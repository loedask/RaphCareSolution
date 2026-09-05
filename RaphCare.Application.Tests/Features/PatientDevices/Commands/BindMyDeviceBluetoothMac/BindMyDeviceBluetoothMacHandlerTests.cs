using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientDevices.Commands.BindMyDeviceBluetoothMac;
using RaphCare.Domain.Common;
using RaphCare.Domain.Devices;
using Xunit;
using ValidationException = RaphCare.Application.Common.Exceptions.ValidationException;

namespace RaphCare.Application.Tests.Features.PatientDevices.Commands.BindMyDeviceBluetoothMac;

public sealed class BindMyDeviceBluetoothMacHandlerTests
{
    [Fact]
    public async Task BindSetsMacWhenEmptyForAssignedPatient()
    {
        var patientId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa1001");
        var deviceId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc1001");
        var device = new Device
        {
            ClinicId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb1001"),
            SerialNumber = "RC-E585-MAC1",
            Model = "E585",
            IsActive = true,
            IsAssigned = true,
            Status = "Assigned",
            BluetoothMacAddress = null
        };
        EntityId.SetId(device, deviceId);
        var assignment = new DeviceAssignment
        {
            DeviceId = deviceId,
            PatientId = patientId,
            AssignedAt = DateTime.UtcNow,
            IsActive = true
        };
        var devices = new FakeRepository<Device>([device]);
        var assignments = new FakeRepository<DeviceAssignment>([assignment]);
        var handler = CreateHandler(devices, assignments, patientId);

        var result = await handler.Handle(
            new BindMyDeviceBluetoothMacCommand
            {
                DeviceId = deviceId,
                BluetoothMacAddress = "aa:bb:cc:dd:ee:01"
            },
            CancellationToken.None);

        Assert.Equal("AA:BB:CC:DD:EE:01", result.BluetoothMacAddress);
        Assert.Equal("AA:BB:CC:DD:EE:01", devices.Items[0].BluetoothMacAddress);
    }

    [Fact]
    public async Task BindRejectsDifferentMacWhenAlreadyLockedDoesNotOverwrite()
    {
        var patientId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa1002");
        var deviceId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc1002");
        var device = new Device
        {
            ClinicId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb1002"),
            SerialNumber = "RC-E585-MAC2",
            Model = "E585",
            IsActive = true,
            IsAssigned = true,
            Status = "Assigned",
            BluetoothMacAddress = "AA:BB:CC:DD:EE:02"
        };
        EntityId.SetId(device, deviceId);
        var assignment = new DeviceAssignment
        {
            DeviceId = deviceId,
            PatientId = patientId,
            AssignedAt = DateTime.UtcNow,
            IsActive = true
        };
        var devices = new FakeRepository<Device>([device]);
        var assignments = new FakeRepository<DeviceAssignment>([assignment]);
        var handler = CreateHandler(devices, assignments, patientId);

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(
                new BindMyDeviceBluetoothMacCommand
                {
                    DeviceId = deviceId,
                    BluetoothMacAddress = "11:22:33:44:55:66"
                },
                CancellationToken.None));

        Assert.Contains(
            ex.Errors.Values.SelectMany(v => v),
            m => m.Contains("already locked", StringComparison.OrdinalIgnoreCase));
        Assert.Equal("AA:BB:CC:DD:EE:02", devices.Items[0].BluetoothMacAddress);
    }

    [Fact]
    public async Task BindRejectsWhenPatientHasNoAssignment()
    {
        var patientId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa1003");
        var otherPatient = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa1999");
        var deviceId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc1003");
        var device = new Device
        {
            ClinicId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb1003"),
            SerialNumber = "RC-E585-MAC3",
            Model = "E585",
            IsActive = true,
            IsAssigned = true,
            Status = "Assigned"
        };
        EntityId.SetId(device, deviceId);
        var assignment = new DeviceAssignment
        {
            DeviceId = deviceId,
            PatientId = otherPatient,
            AssignedAt = DateTime.UtcNow,
            IsActive = true
        };
        var devices = new FakeRepository<Device>([device]);
        var assignments = new FakeRepository<DeviceAssignment>([assignment]);
        var handler = CreateHandler(devices, assignments, patientId);

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(
                new BindMyDeviceBluetoothMacCommand
                {
                    DeviceId = deviceId,
                    BluetoothMacAddress = "AA:BB:CC:DD:EE:03"
                },
                CancellationToken.None));

        Assert.Contains(
            ex.Errors.Values.SelectMany(v => v),
            m => m.Contains("Claim this watch", StringComparison.OrdinalIgnoreCase));
        Assert.Null(devices.Items[0].BluetoothMacAddress);
    }

    private static BindMyDeviceBluetoothMacHandler CreateHandler(
        IRepository<Device> devices,
        IRepository<DeviceAssignment> assignments,
        Guid patientId)
    {
        return new BindMyDeviceBluetoothMacHandler(
            devices,
            assignments,
            new FakeCurrentUser(patientId),
            new FakeUnitOfWork());
    }
}

file static class EntityId
{
    public static void SetId(BaseEntity entity, Guid id) =>
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(entity, id);
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
