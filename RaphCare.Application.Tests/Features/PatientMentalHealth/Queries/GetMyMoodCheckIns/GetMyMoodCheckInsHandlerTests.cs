using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientMentalHealth.Queries.GetMyMoodCheckIns;
using RaphCare.Domain.MentalHealth;
using Xunit;

namespace RaphCare.Application.Tests.Features.PatientMentalHealth.Queries.GetMyMoodCheckIns;

public sealed class GetMyMoodCheckInsHandlerTests
{
    [Fact]
    public async Task ReturnsNewestFirstForCurrentPatientOnly()
    {
        var patientId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0201");
        var other = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0201");
        var older = new MoodLog
        {
            PatientId = patientId,
            MoodScore = 1,
            LoggedAt = new DateTime(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc)
        };
        var newer = new MoodLog
        {
            PatientId = patientId,
            MoodScore = 0,
            LoggedAt = new DateTime(2026, 9, 5, 12, 0, 0, DateTimeKind.Utc)
        };
        var otherRow = new MoodLog
        {
            PatientId = other,
            MoodScore = 3,
            LoggedAt = new DateTime(2026, 9, 5, 13, 0, 0, DateTimeKind.Utc)
        };

        var handler = new GetMyMoodCheckInsHandler(
            new FakeRepository<MoodLog>([older, newer, otherRow]),
            new FakeCurrentUser(patientId));

        var result = await handler.Handle(new GetMyMoodCheckInsQuery { PageSize = 10 }, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(0, result[0].MoodScore);
        Assert.Equal(newer.LoggedAt, result[0].LoggedAt);
        Assert.Equal(1, result[1].MoodScore);
    }

    [Fact]
    public async Task RequiresPatientProfile()
    {
        var handler = new GetMyMoodCheckInsHandler(
            new FakeRepository<MoodLog>(),
            new FakeCurrentUser(null));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new GetMyMoodCheckInsQuery(), CancellationToken.None));
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
