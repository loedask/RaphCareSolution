using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientDevices.Commands.RegisterMyDevice;
using RaphCare.Domain.Devices;
using RaphCare.Domain.Organization;
using Xunit;
using ValidationException = RaphCare.Application.Common.Exceptions.ValidationException;

namespace RaphCare.Application.Tests.Features.PatientDevices.Commands.RegisterMyDevice;

public sealed class RegisterMyDeviceHandlerTests
{
    [Fact]
    public async Task ClaimRejectsUnknownSerialDoesNotCreateInventory()
    {
        var patientId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0001");
        var clinicId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0001");
        var devices = new FakeRepository<Device>();
        var assignments = new FakeRepository<DeviceAssignment>();
        var handler = CreateHandler(devices, assignments, patientId, [clinicId], clinics: []);

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(
                new RegisterMyDeviceCommand { SerialNumber = "UNKNOWN-SERIAL", ModelSku = "E585" },
                CancellationToken.None));

        Assert.Contains(ex.Errors.Values.SelectMany(v => v), m => m.Contains("not in the RaphCare fleet", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(devices.Items);
        Assert.Empty(assignments.Items);
    }

    [Fact]
    public async Task ClaimSucceedsWhenDeviceAssignedToPatient()
    {
        var patientId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0002");
        var clinicId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0002");
        var device = new Device
        {
            ClinicId = clinicId,
            SerialNumber = "RC-E585-1001",
            Model = "E585",
            IsActive = true,
            IsAssigned = true,
            Status = "Assigned"
        };
        EntityId.SetId(device, Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0002"));
        var assignment = new DeviceAssignment
        {
            DeviceId = device.Id,
            PatientId = patientId,
            AssignedAt = DateTime.UtcNow,
            IsActive = true
        };
        var clinic = Clinic(clinicId, allowSelfClaim: false);
        var devices = new FakeRepository<Device>([device]);
        var assignments = new FakeRepository<DeviceAssignment>([assignment]);
        var handler = CreateHandler(devices, assignments, patientId, [clinicId], [clinic]);

        var result = await handler.Handle(
            new RegisterMyDeviceCommand { SerialNumber = "RC-E585-1001", ModelSku = "E585" },
            CancellationToken.None);

        Assert.Equal(device.Id, result.DeviceId);
        Assert.Single(devices.Items);
        Assert.Single(assignments.Items);
    }

    [Fact]
    public async Task ClaimRejectsWhenInStockAtHospitalWithoutPriorAssignment()
    {
        var patientId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0003");
        var clinicId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0003");
        var device = new Device
        {
            ClinicId = clinicId,
            SerialNumber = "RC-E580-2002",
            Model = "E580",
            IsActive = true,
            IsAssigned = false,
            Status = "InStock"
        };
        EntityId.SetId(device, Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0003"));
        var handler = CreateHandler(
            new FakeRepository<Device>([device]),
            new FakeRepository<DeviceAssignment>(),
            patientId,
            [clinicId],
            [Clinic(clinicId, allowSelfClaim: false)]);

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(
                new RegisterMyDeviceCommand { SerialNumber = "RC-E580-2002", ModelSku = "E580" },
                CancellationToken.None));

        Assert.Contains(ex.Errors.Values.SelectMany(v => v), m => m.Contains("not assigned to you", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task PackagingSelfClaimCreatesAssignmentForDirectInStockDevice()
    {
        var patientId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0004");
        var clinicId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0004");
        var device = new Device
        {
            ClinicId = clinicId,
            SerialNumber = "RC-E585-DIRECT-1",
            Model = "E585",
            IsActive = true,
            IsAssigned = false,
            Status = "InStock"
        };
        EntityId.SetId(device, Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0004"));
        var devices = new FakeRepository<Device>([device]);
        var assignments = new FakeRepository<DeviceAssignment>();
        var access = new FakePatientAccess([]);
        var handler = CreateHandler(
            devices,
            assignments,
            patientId,
            access,
            [Clinic(clinicId, allowSelfClaim: true)]);

        var result = await handler.Handle(
            new RegisterMyDeviceCommand { SerialNumber = "RC-E585-DIRECT-1", ModelSku = "E585" },
            CancellationToken.None);

        Assert.Equal(device.Id, result.DeviceId);
        Assert.True(device.IsAssigned);
        Assert.Equal("Assigned", device.Status);
        Assert.Single(assignments.Items);
        Assert.Contains(clinicId, access.GrantedClinicIds);
    }

    private static RegisterMyDeviceHandler CreateHandler(
        IRepository<Device> devices,
        IRepository<DeviceAssignment> assignments,
        Guid patientId,
        Guid[] clinicIds,
        IEnumerable<Clinic> clinics) =>
        CreateHandler(devices, assignments, patientId, new FakePatientAccess(clinicIds), clinics);

    private static RegisterMyDeviceHandler CreateHandler(
        IRepository<Device> devices,
        IRepository<DeviceAssignment> assignments,
        Guid patientId,
        IPatientClinicAccessService access,
        IEnumerable<Clinic> clinics) =>
        new(
            devices,
            assignments,
            new FakeRepository<Clinic>(clinics),
            new FakeCurrentUser(patientId),
            access,
            new FakeUnitOfWork(),
            new FakeClock());

    private static Clinic Clinic(Guid id, bool allowSelfClaim)
    {
        var clinic = new Clinic
        {
            Name = "Test",
            AllowPatientDeviceSelfClaim = allowSelfClaim,
            IsActive = true
        };
        EntityId.SetId(clinic, id);
        return clinic;
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

file sealed class FakeCurrentUser(Guid patientId) : ICurrentUserService
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
    private readonly HashSet<Guid> _clinicIds = [.. clinicIds];
    public List<Guid> GrantedClinicIds { get; } = [];

    public Task<bool> HasClinicAccessAsync(Guid patientId, Guid clinicId, CancellationToken ct) =>
        Task.FromResult(_clinicIds.Contains(clinicId));

    public Task EnsureClinicAccessAsync(Guid patientId, Guid clinicId, CancellationToken ct) =>
        _clinicIds.Contains(clinicId) ? Task.CompletedTask : throw new ForbiddenAccessException("No access.");

    public Task GrantEncounterAccessAsync(Guid patientId, Guid clinicId, CancellationToken ct) =>
        Task.CompletedTask;

    public Task<Guid[]> GetAccessibleClinicIdsAsync(Guid patientId, CancellationToken ct) =>
        Task.FromResult(_clinicIds.ToArray());

    public Task GrantManualAccessAsync(Guid patientId, Guid clinicId, string? notes, CancellationToken ct)
    {
        _clinicIds.Add(clinicId);
        GrantedClinicIds.Add(clinicId);
        return Task.CompletedTask;
    }

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
