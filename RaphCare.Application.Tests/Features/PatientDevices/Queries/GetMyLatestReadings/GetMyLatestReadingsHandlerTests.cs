using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientDevices.Queries.GetMyLatestReadings;
using RaphCare.Domain.Devices;
using Xunit;

namespace RaphCare.Application.Tests.Features.PatientDevices.Queries.GetMyLatestReadings;

public sealed class GetMyLatestReadingsHandlerTests
{
    [Fact]
    public async Task ReturnsLatestHeartRateAndSpO2PerType()
    {
        var patientId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0101");
        var otherPatient = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0101");
        var olderHr = new HeartRateReading
        {
            PatientId = patientId,
            DeviceId = Guid.NewGuid(),
            ReadingType = "HeartRate",
            PrimaryValue = 60,
            HeartRate = 60,
            RecordedAt = new DateTime(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc),
            ReceivedAt = DateTime.UtcNow,
            Unit = "bpm"
        };
        var newerHr = new HeartRateReading
        {
            PatientId = patientId,
            DeviceId = Guid.NewGuid(),
            ReadingType = "HeartRate",
            PrimaryValue = 78,
            HeartRate = 78,
            RecordedAt = new DateTime(2026, 9, 5, 8, 0, 0, DateTimeKind.Utc),
            ReceivedAt = DateTime.UtcNow,
            Unit = "bpm"
        };
        var spo2 = new PulseOximeterReading
        {
            PatientId = patientId,
            DeviceId = Guid.NewGuid(),
            ReadingType = "SpO2",
            PrimaryValue = 97,
            SpO2 = 97,
            RecordedAt = new DateTime(2026, 9, 5, 7, 30, 0, DateTimeKind.Utc),
            ReceivedAt = DateTime.UtcNow,
            Unit = "%"
        };
        var other = new HeartRateReading
        {
            PatientId = otherPatient,
            DeviceId = Guid.NewGuid(),
            ReadingType = "HeartRate",
            PrimaryValue = 120,
            HeartRate = 120,
            RecordedAt = new DateTime(2026, 9, 5, 9, 0, 0, DateTimeKind.Utc),
            ReceivedAt = DateTime.UtcNow,
            Unit = "bpm"
        };

        var handler = new GetMyLatestReadingsHandler(
            new FakeRepository<DeviceReading>([olderHr, newerHr, spo2, other]),
            new FakeCurrentUser(patientId));

        var result = await handler.Handle(new GetMyLatestReadingsQuery(), CancellationToken.None);

        Assert.Equal(78, result.HeartRateBpm);
        Assert.Equal(newerHr.RecordedAt, result.HeartRateRecordedAt);
        Assert.Equal(97, result.SpO2Percent);
        Assert.Equal(spo2.RecordedAt, result.SpO2RecordedAt);
    }

    [Fact]
    public async Task RequiresPatientProfile()
    {
        var handler = new GetMyLatestReadingsHandler(
            new FakeRepository<DeviceReading>(),
            new FakeCurrentUser(null));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new GetMyLatestReadingsQuery(), CancellationToken.None));
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

file sealed class FakeRepository<T> : IRepository<T> where T : class
{
    public List<T> Items { get; }

    public FakeRepository(IEnumerable<T>? items = null)
    {
        Items = items?.ToList() ?? [];
    }

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult<T?>(null);

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
