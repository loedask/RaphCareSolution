using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Devices.Commands.DeleteDevice;
using RaphCare.Application.Features.Devices.Commands.UnassignDeviceFromPatient;
using RaphCare.Domain.Devices;
using Xunit;
using ValidationException = RaphCare.Application.Common.Exceptions.ValidationException;

namespace RaphCare.Application.Tests.Features.Devices.Commands;

public sealed class UnassignAndDeleteDeviceHandlerTests
{
    private static readonly DateTime UtcNow = new(2026, 9, 6, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task UnassignEndsActiveAssignmentAndReturnsDeviceToStock()
    {
        var deviceId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0001");
        var patientId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0001");
        var device = new Device
        {
            ClinicId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0001"),
            SerialNumber = "RC-E585-REV-1",
            IsActive = true,
            IsAssigned = true,
            Status = "Assigned"
        };
        EntityId.SetId(device, deviceId);
        var assignment = new DeviceAssignment
        {
            DeviceId = deviceId,
            PatientId = patientId,
            AssignedAt = UtcNow.AddDays(-2),
            IsActive = true
        };
        EntityId.SetId(assignment, Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddd0001"));

        var devices = new FakeRepository<Device>([device]);
        var assignments = new FakeRepository<DeviceAssignment>([assignment]);
        var handler = new UnassignDeviceFromPatientHandler(
            devices,
            assignments,
            new FakeUnitOfWork(),
            new FakeClock(UtcNow));

        var result = await handler.Handle(
            new UnassignDeviceFromPatientCommand { DeviceId = deviceId },
            CancellationToken.None);

        Assert.Equal(deviceId, result);
        Assert.False(device.IsAssigned);
        Assert.Equal("InStock", device.Status);
        Assert.False(assignment.IsActive);
        Assert.Equal(UtcNow, assignment.ReturnedAt);
    }

    [Fact]
    public async Task UnassignRejectsWhenDeviceHasNoAssignment()
    {
        var deviceId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0002");
        var device = new Device
        {
            ClinicId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0002"),
            SerialNumber = "RC-E585-REV-2",
            IsActive = true,
            IsAssigned = false,
            Status = "InStock"
        };
        EntityId.SetId(device, deviceId);

        var handler = new UnassignDeviceFromPatientHandler(
            new FakeRepository<Device>([device]),
            new FakeRepository<DeviceAssignment>(),
            new FakeUnitOfWork(),
            new FakeClock(UtcNow));

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(new UnassignDeviceFromPatientCommand { DeviceId = deviceId }, CancellationToken.None));

        Assert.Contains(ex.Errors.Values.SelectMany(v => v), m => m.Contains("not assigned", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task DeleteRejectsWhenDeviceStillAssigned()
    {
        var deviceId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0003");
        var device = new Device
        {
            ClinicId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0003"),
            SerialNumber = "RC-E585-DEL-1",
            IsActive = true,
            IsAssigned = true,
            Status = "Assigned"
        };
        EntityId.SetId(device, deviceId);

        var handler = new DeleteDeviceHandler(
            new FakeRepository<Device>([device]),
            new FakeRepository<DeviceAssignment>(),
            new FakeRepository<DeviceReading>(),
            new FakeRepository<DeviceEmergencyEvent>(),
            new FakeUnitOfWork());

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(new DeleteDeviceCommand { DeviceId = deviceId }, CancellationToken.None));

        Assert.Contains(ex.Errors.Values.SelectMany(v => v), m => m.Contains("Revoke", StringComparison.OrdinalIgnoreCase));
        Assert.True(device.IsActive);
    }

    [Fact]
    public async Task DeleteHardRemovesCleanUnassignedStock()
    {
        var deviceId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0004");
        var device = new Device
        {
            ClinicId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0004"),
            SerialNumber = "RC-E585-DEL-2",
            IsActive = true,
            IsAssigned = false,
            Status = "InStock"
        };
        EntityId.SetId(device, deviceId);
        var devices = new FakeRepository<Device>([device]);

        var handler = new DeleteDeviceHandler(
            devices,
            new FakeRepository<DeviceAssignment>(),
            new FakeRepository<DeviceReading>(),
            new FakeRepository<DeviceEmergencyEvent>(),
            new FakeUnitOfWork());

        await handler.Handle(new DeleteDeviceCommand { DeviceId = deviceId }, CancellationToken.None);

        Assert.Empty(devices.Items);
    }

    [Fact]
    public async Task DeleteRetiresDeviceWhenAssignmentHistoryExists()
    {
        var deviceId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0005");
        var device = new Device
        {
            ClinicId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0005"),
            SerialNumber = "RC-E585-DEL-3",
            IsActive = true,
            IsAssigned = false,
            Status = "InStock"
        };
        EntityId.SetId(device, deviceId);
        var past = new DeviceAssignment
        {
            DeviceId = deviceId,
            PatientId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0005"),
            AssignedAt = UtcNow.AddDays(-10),
            ReturnedAt = UtcNow.AddDays(-1),
            IsActive = false
        };
        EntityId.SetId(past, Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddd0005"));
        var devices = new FakeRepository<Device>([device]);

        var handler = new DeleteDeviceHandler(
            devices,
            new FakeRepository<DeviceAssignment>([past]),
            new FakeRepository<DeviceReading>(),
            new FakeRepository<DeviceEmergencyEvent>(),
            new FakeUnitOfWork());

        await handler.Handle(new DeleteDeviceCommand { DeviceId = deviceId }, CancellationToken.None);

        Assert.Single(devices.Items);
        Assert.False(device.IsActive);
        Assert.Equal("Retired", device.Status);
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

file sealed class FakeClock(DateTime utcNow) : IDateTimeProvider
{
    public DateTime UtcNow => utcNow;
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
