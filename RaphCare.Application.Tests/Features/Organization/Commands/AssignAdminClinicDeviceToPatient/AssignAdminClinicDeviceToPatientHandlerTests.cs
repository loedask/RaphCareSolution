using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.Commands.AssignAdminClinicDeviceToPatient;
using RaphCare.Domain.Devices;
using RaphCare.Domain.Patients;
using Xunit;
using ValidationException = RaphCare.Application.Common.Exceptions.ValidationException;

namespace RaphCare.Application.Tests.Features.Organization.Commands.AssignAdminClinicDeviceToPatient;

public sealed class AssignAdminClinicDeviceToPatientHandlerTests
{
    [Fact]
    public async Task AssignRejectsPatientWithoutClinicAccess()
    {
        var clinicId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddd0001");
        var deviceId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeee0001");
        var patientId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffff00000001");
        var device = new Device
        {
            ClinicId = clinicId,
            SerialNumber = "RC-E585-HOSP-1",
            IsActive = true,
            IsAssigned = false,
            Status = "InStock"
        };
        EntityId.SetId(device, deviceId);
        var patient = new Patient { FirstName = "Ada", LastName = "Patient" };
        EntityId.SetId(patient, patientId);

        var handler = new AssignAdminClinicDeviceToPatientHandler(
            new FakeCurrentUser(),
            new FakeStaffAccess(hasAccess: true),
            new FakePatientAccess(hasAccess: false),
            new FakeRepository<Device>([device]),
            new FakeRepository<DeviceAssignment>(),
            new FakeRepository<Patient>([patient]),
            new FakeUnitOfWork(),
            new FakeClock());

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(
                new AssignAdminClinicDeviceToPatientCommand
                {
                    ClinicId = clinicId,
                    DeviceId = deviceId,
                    PatientId = patientId
                },
                CancellationToken.None));

        Assert.Contains(ex.Errors.Values.SelectMany(v => v), m => m.Contains("not linked to this hospital", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task AssignCreatesAssignmentWhenStaffAndPatientBelongToClinic()
    {
        var clinicId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddd0002");
        var deviceId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeee0002");
        var patientId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffff00000002");
        var device = new Device
        {
            ClinicId = clinicId,
            SerialNumber = "RC-E585-HOSP-2",
            IsActive = true,
            IsAssigned = false,
            Status = "InStock"
        };
        EntityId.SetId(device, deviceId);
        var patient = new Patient { FirstName = "Bea", LastName = "Patient" };
        EntityId.SetId(patient, patientId);
        var devices = new FakeRepository<Device>([device]);
        var assignments = new FakeRepository<DeviceAssignment>();

        var handler = new AssignAdminClinicDeviceToPatientHandler(
            new FakeCurrentUser(),
            new FakeStaffAccess(hasAccess: true),
            new FakePatientAccess(hasAccess: true),
            devices,
            assignments,
            new FakeRepository<Patient>([patient]),
            new FakeUnitOfWork(),
            new FakeClock());

        var result = await handler.Handle(
            new AssignAdminClinicDeviceToPatientCommand
            {
                ClinicId = clinicId,
                DeviceId = deviceId,
                PatientId = patientId
            },
            CancellationToken.None);

        Assert.Equal(deviceId, result);
        Assert.True(device.IsAssigned);
        Assert.Equal("Assigned", device.Status);
        Assert.Single(assignments.Items);
    }
}

file static class EntityId
{
    public static void SetId<T>(T entity, Guid id)
    {
        var prop = typeof(T).GetProperty("Id")
            ?? throw new InvalidOperationException($"{typeof(T).Name} has no Id property.");
        prop.SetValue(entity, id);
    }
}

file sealed class FakeCurrentUser : ICurrentUserService
{
    public string? UserId { get; } = Guid.NewGuid().ToString();
    public Guid? CurrentUserId { get; } = Guid.NewGuid();
    public string? UserName => "admin";
    public Guid? CurrentPatientId => null;
    public bool IsAuthenticated => true;
    public string? IpAddress => null;
    public string? UserAgent => null;
}

file sealed class FakeStaffAccess(bool hasAccess) : IClinicStaffMembershipService
{
    public Task EnsureMembershipAsync(Guid applicationUserId, Guid clinicId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task<IReadOnlyList<Guid>> GetClinicIdsForUserAsync(Guid applicationUserId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Guid>>(hasAccess ? [Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddd0002")] : []);

    public Task<bool> HasMembershipAsync(Guid applicationUserId, Guid clinicId, CancellationToken cancellationToken = default) =>
        Task.FromResult(hasAccess);

    public Task<int> GetActiveStaffCountAsync(Guid clinicId, CancellationToken cancellationToken = default) =>
        Task.FromResult(hasAccess ? 1 : 0);

    public Task<IReadOnlyList<ClinicStaffMembershipEntry>> GetStaffMembershipsAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ClinicStaffMembershipEntry>>([]);

    public Task<bool> DeactivateMembershipAsync(
        Guid applicationUserId,
        Guid clinicId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(false);

    public Task RecordInvitationSentAsync(Guid applicationUserId, Guid clinicId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

file sealed class FakePatientAccess(bool hasAccess) : IPatientClinicAccessService
{
    public Task<bool> HasClinicAccessAsync(Guid patientId, Guid clinicId, CancellationToken ct) =>
        Task.FromResult(hasAccess);

    public Task EnsureClinicAccessAsync(Guid patientId, Guid clinicId, CancellationToken ct) =>
        hasAccess ? Task.CompletedTask : throw new ForbiddenAccessException("No access.");

    public Task GrantEncounterAccessAsync(Guid patientId, Guid clinicId, CancellationToken ct) =>
        Task.CompletedTask;

    public Task<Guid[]> GetAccessibleClinicIdsAsync(Guid patientId, CancellationToken ct) =>
        Task.FromResult(Array.Empty<Guid>());

    public Task GrantManualAccessAsync(Guid patientId, Guid clinicId, string? notes, CancellationToken ct) =>
        Task.CompletedTask;

    public Task RevokeClinicAccessAsync(Guid patientId, Guid clinicId, CancellationToken ct) =>
        Task.CompletedTask;
}

file sealed class FakeClock : IDateTimeProvider
{
    public DateTime UtcNow { get; } = DateTime.UtcNow;
}

file sealed class FakeUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
}

file sealed class FakeRepository<T> : IRepository<T> where T : class
{
    public List<T> Items { get; }

    public FakeRepository(IEnumerable<T>? items = null) => Items = items?.ToList() ?? [];

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
