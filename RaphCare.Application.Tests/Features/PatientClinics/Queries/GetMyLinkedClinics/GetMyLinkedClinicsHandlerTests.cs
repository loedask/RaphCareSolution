using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientClinics.Queries.GetMyLinkedClinics;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Patients;
using RaphCare.Domain.Patients.Enums;
using Xunit;

namespace RaphCare.Application.Tests.Features.PatientClinics.Queries.GetMyLinkedClinics;

public sealed class GetMyLinkedClinicsHandlerTests
{
    [Fact]
    public async Task ReturnsAccessibleClinicsWithAccessKind()
    {
        var patientId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0301");
        var linkedId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0301");
        var careOnlyId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0301");
        var otherId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddd0301");

        var linkedClinic = Clinic(linkedId, "Demo Clinic", "RC-DEMCLN");
        var careClinic = Clinic(careOnlyId, "Care History Hospital", "RC-CARE01");
        var otherClinic = Clinic(otherId, "Other", "RC-OTHER");

        var accessRow = new PatientClinicAccess
        {
            PatientId = patientId,
            ClinicId = linkedId,
            AccessType = PatientClinicAccessType.Registered,
            GrantedAt = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
            IsActive = true,
            GrantedByRule = "Registered"
        };

        var handler = new GetMyLinkedClinicsHandler(
            new FakePatientAccess([linkedId, careOnlyId]),
            new FakeRepository<Clinic>([linkedClinic, careClinic, otherClinic]),
            new FakeRepository<PatientClinicAccess>([accessRow]),
            new FakeCurrentUser(patientId));

        var result = await handler.Handle(new GetMyLinkedClinicsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(careOnlyId, result[0].ClinicId); // Care History Hospital sorts before Demo
        Assert.Equal("CareHistory", result[0].AccessKind);
        Assert.Equal(linkedId, result[1].ClinicId);
        Assert.Equal("Registered", result[1].AccessKind);
        Assert.Equal("RC-DEMCLN", result[1].ReferenceCode);
    }

    [Fact]
    public async Task RequiresPatientProfile()
    {
        var handler = new GetMyLinkedClinicsHandler(
            new FakePatientAccess([]),
            new FakeRepository<Clinic>(),
            new FakeRepository<PatientClinicAccess>(),
            new FakeCurrentUser(null));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new GetMyLinkedClinicsQuery(), CancellationToken.None));
    }

    private static Clinic Clinic(Guid id, string name, string code)
    {
        var clinic = new Clinic
        {
            Name = name,
            ReferenceCode = code,
            IsActive = true,
            IsDeleted = false
        };
        EntityId.SetId(clinic, id);
        return clinic;
    }
}

file sealed class FakeCurrentUser(Guid? patientId) : ICurrentUserService
{
    public string? UserId { get; } = Guid.NewGuid().ToString();
    public Guid? CurrentUserId { get; } = Guid.NewGuid();
    public string? UserName => "patient";
    public Guid? CurrentPatientId { get; } = patientId;
    public bool IsAuthenticated => true;
    public string? IpAddress => null;
    public string? UserAgent => null;
}

file sealed class FakePatientAccess(Guid[] clinicIds) : IPatientClinicAccessService
{
    public Task<bool> HasClinicAccessAsync(Guid patientId, Guid clinicId, CancellationToken ct) =>
        Task.FromResult(clinicIds.Contains(clinicId));

    public Task EnsureClinicAccessAsync(Guid patientId, Guid clinicId, CancellationToken ct) =>
        clinicIds.Contains(clinicId) ? Task.CompletedTask : throw new ForbiddenAccessException("No access.");

    public Task GrantEncounterAccessAsync(Guid patientId, Guid clinicId, CancellationToken ct) =>
        Task.CompletedTask;

    public Task<Guid[]> GetAccessibleClinicIdsAsync(Guid patientId, CancellationToken ct) =>
        Task.FromResult(clinicIds);

    public Task GrantManualAccessAsync(Guid patientId, Guid clinicId, string? notes, CancellationToken ct) =>
        Task.CompletedTask;

    public Task RevokeClinicAccessAsync(Guid patientId, Guid clinicId, CancellationToken ct) =>
        Task.CompletedTask;
}

file static class EntityId
{
    public static void SetId<T>(T entity, Guid id) where T : class
    {
        var prop = typeof(T).GetProperty("Id")
            ?? throw new InvalidOperationException($"{typeof(T).Name} has no Id property.");
        prop.SetValue(entity, id);
    }
}

file sealed class FakeRepository<T> : IRepository<T> where T : class
{
    public List<T> Items { get; }

    public FakeRepository(IEnumerable<T>? items = null)
    {
        Items = items?.ToList() ?? [];
    }

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var match = Items.FirstOrDefault(item =>
        {
            var value = typeof(T).GetProperty("Id")?.GetValue(item);
            return value is Guid guid && guid == id;
        });
        return Task.FromResult(match);
    }

    public Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<T>>(Items);

    public Task<PagedResult<T>> SearchAsync(
        Func<IQueryable<T>, IQueryable<T>>? queryShaper,
        int pageNumber,
        int pageSize,
        bool applyDefaultIdOrdering = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = Items.AsQueryable();
        if (queryShaper is not null)
            query = queryShaper(query);
        var list = query.ToList();
        return Task.FromResult(new PagedResult<T>
        {
            Items = list.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
            TotalCount = list.Count,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    public Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        Items.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        Items.Remove(entity);
        return Task.CompletedTask;
    }
}
