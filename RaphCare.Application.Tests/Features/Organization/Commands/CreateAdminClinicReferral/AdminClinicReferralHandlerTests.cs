using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.Commands.CreateAdminClinicReferral;
using RaphCare.Application.Features.Organization.Commands.UpdateAdminClinicReferralStatus;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Patients;
using Xunit;

namespace RaphCare.Application.Tests.Features.Organization.Commands.CreateAdminClinicReferral;

public sealed class AdminClinicReferralHandlerTests
{
    [Fact]
    public async Task CreateReferralStartsAsSent()
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111301");
        var patientId = Guid.Parse("33333333-3333-3333-3333-333333333301");
        var patient = new Patient { FirstName = "Paul", LastName = "N." };
        EntityId.SetId(patient, patientId);
        var referrals = new FakeRepository<Referral>();
        var now = new DateTime(2026, 9, 2, 15, 0, 0, DateTimeKind.Utc);
        var handler = new CreateAdminClinicReferralHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            referrals,
            new FakeRepository<Patient>([patient]),
            new FakeRepository<Visit>(),
            new FakeClock(now),
            new FakeUnitOfWork());

        var result = await handler.Handle(
            new CreateAdminClinicReferralCommand
            {
                ClinicId = clinicId,
                PatientId = patientId,
                ReferredTo = "Kinshasa General Hospital",
                Specialty = "Cardiology",
                Reason = "Chest pain workup"
            },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Sent", result!.Status);
        Assert.Equal("Kinshasa General Hospital", result.ReferredTo);
        Assert.Equal(now, result.ReferredAt);
        Assert.Single(referrals.Items);
    }

    [Fact]
    public async Task UpdateReferralStatusAcceptsThenCompletes()
    {
        var clinicId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var patient = new Patient { FirstName = "Amina", LastName = "K." };
        EntityId.SetId(patient, patientId);
        var referral = new Referral
        {
            ClinicId = clinicId,
            PatientId = patientId,
            ReferredTo = "City Clinic",
            Status = "Sent",
            ReferredAt = DateTime.UtcNow.AddDays(-1)
        };
        var referrals = new FakeRepository<Referral>([referral]);
        var now = new DateTime(2026, 9, 2, 16, 0, 0, DateTimeKind.Utc);
        var handler = new UpdateAdminClinicReferralStatusHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            referrals,
            new FakeRepository<Patient>([patient]),
            new FakeClock(now),
            new FakeUnitOfWork());

        var accepted = await handler.Handle(
            new UpdateAdminClinicReferralStatusCommand
            {
                ClinicId = clinicId,
                ReferralId = referral.Id,
                Status = "Accepted"
            },
            CancellationToken.None);
        Assert.Equal("Accepted", accepted!.Status);
        Assert.Equal(now, referral.AcceptedAt);

        var completed = await handler.Handle(
            new UpdateAdminClinicReferralStatusCommand
            {
                ClinicId = clinicId,
                ReferralId = referral.Id,
                Status = "Completed"
            },
            CancellationToken.None);
        Assert.Equal("Completed", completed!.Status);
        Assert.NotNull(referral.CompletedAt);
    }

    [Fact]
    public async Task UpdateReferralStatusRejectsAcceptWhenNotSent()
    {
        var clinicId = Guid.NewGuid();
        var referral = new Referral
        {
            ClinicId = clinicId,
            PatientId = Guid.NewGuid(),
            ReferredTo = "Elsewhere",
            Status = "Accepted",
            ReferredAt = DateTime.UtcNow
        };
        var handler = new UpdateAdminClinicReferralStatusHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            new FakeRepository<Referral>([referral]),
            new FakeRepository<Patient>(),
            new FakeClock(DateTime.UtcNow),
            new FakeUnitOfWork());

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.Handle(
                new UpdateAdminClinicReferralStatusCommand
                {
                    ClinicId = clinicId,
                    ReferralId = referral.Id,
                    Status = "Accepted"
                },
                CancellationToken.None));
    }

    [Fact]
    public async Task UpdateReferralStatusRejectsRestartAfterComplete()
    {
        var clinicId = Guid.NewGuid();
        var referral = new Referral
        {
            ClinicId = clinicId,
            PatientId = Guid.NewGuid(),
            ReferredTo = "Done",
            Status = "Completed",
            ReferredAt = DateTime.UtcNow.AddDays(-2),
            CompletedAt = DateTime.UtcNow.AddDays(-1)
        };
        var handler = new UpdateAdminClinicReferralStatusHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            new FakeRepository<Referral>([referral]),
            new FakeRepository<Patient>(),
            new FakeClock(DateTime.UtcNow),
            new FakeUnitOfWork());

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.Handle(
                new UpdateAdminClinicReferralStatusCommand
                {
                    ClinicId = clinicId,
                    ReferralId = referral.Id,
                    Status = "Accepted"
                },
                CancellationToken.None));
    }
}

file static class EntityId
{
    public static void SetId<T>(T entity, Guid id) where T : class
    {
        var property = typeof(T).GetProperty("Id")
            ?? throw new InvalidOperationException($"{typeof(T).Name} has no Id.");
        property.SetValue(entity, id);
    }
}

file sealed class FakeClock(DateTime utcNow) : IDateTimeProvider
{
    public DateTime UtcNow { get; } = utcNow;
}

file sealed class FakeUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
}

file sealed class FakeCurrentUser(Guid userId) : ICurrentUserService
{
    public string? UserId { get; } = userId.ToString();
    public Guid? CurrentUserId { get; } = userId;
    public string? UserName => "tester";
    public Guid? CurrentPatientId => null;
    public bool IsAuthenticated => true;
    public string? IpAddress => null;
    public string? UserAgent => null;
}

file sealed class FakeMembership(bool hasMembership) : IClinicStaffMembershipService
{
    public Task EnsureMembershipAsync(Guid applicationUserId, Guid clinicId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task<IReadOnlyList<Guid>> GetClinicIdsForUserAsync(Guid applicationUserId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Guid>>([]);

    public Task<bool> HasMembershipAsync(Guid applicationUserId, Guid clinicId, CancellationToken cancellationToken = default) =>
        Task.FromResult(hasMembership);

    public Task<int> GetActiveStaffCountAsync(Guid clinicId, CancellationToken cancellationToken = default) =>
        Task.FromResult(1);

    public Task<IReadOnlyList<ClinicStaffMembershipEntry>> GetStaffMembershipsAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ClinicStaffMembershipEntry>>([]);

    public Task<bool> DeactivateMembershipAsync(
        Guid applicationUserId,
        Guid clinicId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(true);

    public Task RecordInvitationSentAsync(Guid applicationUserId, Guid clinicId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
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
