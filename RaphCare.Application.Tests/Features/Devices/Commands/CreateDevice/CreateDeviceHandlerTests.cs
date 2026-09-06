using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Devices.Commands.CreateDevice;
using RaphCare.Domain.Devices;
using Xunit;
using ValidationException = RaphCare.Application.Common.Exceptions.ValidationException;

namespace RaphCare.Application.Tests.Features.Devices.Commands.CreateDevice;

public sealed class CreateDeviceHandlerTests
{
    [Fact]
    public async Task CreateRejectsActiveDuplicateSerial()
    {
        var existing = new Device
        {
            ClinicId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0101"),
            SerialNumber = "ET595",
            Model = "E585",
            IsActive = true,
            IsAssigned = false,
            Status = "InStock"
        };
        EntityId.SetId(existing, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0101"));

        var handler = new CreateDeviceHandler(new FakeRepository<Device>([existing]), new FakeUnitOfWork());

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(
                new CreateDeviceCommand
                {
                    ClinicId = existing.ClinicId,
                    SerialNumber = "ET595",
                    Model = "E585"
                },
                CancellationToken.None));

        Assert.Contains(
            ex.Errors.Values.SelectMany(v => v),
            m => m.Contains("already in the fleet", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CreateRestoresRetiredSerialIntoStock()
    {
        var clinicId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0102");
        var newClinicId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0103");
        var existing = new Device
        {
            ClinicId = clinicId,
            SerialNumber = "ET595",
            Model = "E585",
            IsActive = false,
            IsAssigned = false,
            Status = "Retired",
            ActivatedAt = new DateTime(2026, 9, 6, 8, 0, 0, DateTimeKind.Utc)
        };
        EntityId.SetId(existing, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0102"));
        var devices = new FakeRepository<Device>([existing]);

        var handler = new CreateDeviceHandler(devices, new FakeUnitOfWork());

        var id = await handler.Handle(
            new CreateDeviceCommand
            {
                ClinicId = newClinicId,
                SerialNumber = "ET595",
                Model = "E580",
                BluetoothMacAddress = "AA:BB:CC:DD:EE:FF"
            },
            CancellationToken.None);

        Assert.Equal(existing.Id, id);
        Assert.Single(devices.Items);
        Assert.True(existing.IsActive);
        Assert.False(existing.IsAssigned);
        Assert.Equal("InStock", existing.Status);
        Assert.Equal(newClinicId, existing.ClinicId);
        Assert.Equal("E580", existing.Model);
        Assert.Equal("AA:BB:CC:DD:EE:FF", existing.BluetoothMacAddress);
        Assert.Null(existing.ActivatedAt);
    }

    [Fact]
    public async Task CreateRejectsNewStockWithoutBluetoothMac()
    {
        var handler = new CreateDeviceHandler(new FakeRepository<Device>(), new FakeUnitOfWork());

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(
                new CreateDeviceCommand
                {
                    ClinicId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0104"),
                    SerialNumber = "RC-E585-NEW",
                    Model = "E585"
                },
                CancellationToken.None));

        Assert.Contains(
            ex.Errors.Values.SelectMany(v => v),
            m => m.Contains("Bluetooth MAC", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CreateRestoreKeepsExistingMacWhenFormOmitsMac()
    {
        var existing = new Device
        {
            ClinicId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0105"),
            SerialNumber = "RC-E585-KEEP",
            Model = "E585",
            BluetoothMacAddress = "11:22:33:44:55:66",
            IsActive = false,
            IsAssigned = false,
            Status = "Retired"
        };
        EntityId.SetId(existing, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0105"));

        var handler = new CreateDeviceHandler(new FakeRepository<Device>([existing]), new FakeUnitOfWork());

        await handler.Handle(
            new CreateDeviceCommand
            {
                ClinicId = existing.ClinicId,
                SerialNumber = "RC-E585-KEEP",
                Model = "E585"
            },
            CancellationToken.None);

        Assert.Equal("11:22:33:44:55:66", existing.BluetoothMacAddress);
        Assert.True(existing.IsActive);
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
