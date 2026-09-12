using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Appointments;
using RaphCare.Domain.Clinical;
using Xunit;

namespace RaphCare.Application.Tests.Features.Appointments;

public sealed class AppointmentReminderPlannerTests
{
    [Fact]
    public async Task ReplaceUnsentSchedules24hAnd2hWhenBothStillInFuture()
    {
        var repo = new FakeReminderRepository();
        var appointmentId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
        var now = new DateTime(2026, 9, 12, 10, 0, 0, DateTimeKind.Utc);
        var start = now.AddHours(30);

        await AppointmentReminderPlanner.ReplaceUnsentAsync(repo, appointmentId, start, now, CancellationToken.None);

        Assert.Equal(2, repo.Items.Count);
        Assert.All(repo.Items, r =>
        {
            Assert.Equal(appointmentId, r.AppointmentId);
            Assert.Equal(AppointmentReminderChannels.InApp, r.Channel);
            Assert.False(r.Sent);
        });
        Assert.Contains(repo.Items, r => r.ReminderTime == start.AddHours(-24));
        Assert.Contains(repo.Items, r => r.ReminderTime == start.AddHours(-2));
    }

    [Fact]
    public async Task ReplaceUnsentSkipsOffsetsAlreadyPast()
    {
        var repo = new FakeReminderRepository();
        var appointmentId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2");
        var now = new DateTime(2026, 9, 12, 10, 0, 0, DateTimeKind.Utc);
        var start = now.AddHours(3);

        await AppointmentReminderPlanner.ReplaceUnsentAsync(repo, appointmentId, start, now, CancellationToken.None);

        Assert.Single(repo.Items);
        Assert.Equal(start.AddHours(-2), repo.Items[0].ReminderTime);
    }

    [Fact]
    public async Task ClearUnsentRemovesOnlyUnsentRows()
    {
        var appointmentId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3");
        var sent = new AppointmentReminder
        {
            AppointmentId = appointmentId,
            ReminderTime = DateTime.UtcNow.AddHours(-1),
            Channel = AppointmentReminderChannels.InApp,
            Sent = true
        };
        var unsent = new AppointmentReminder
        {
            AppointmentId = appointmentId,
            ReminderTime = DateTime.UtcNow.AddHours(1),
            Channel = AppointmentReminderChannels.InApp,
            Sent = false
        };
        var repo = new FakeReminderRepository([sent, unsent]);

        await AppointmentReminderPlanner.ClearUnsentAsync(repo, appointmentId, CancellationToken.None);

        Assert.Single(repo.Items);
        Assert.True(repo.Items[0].Sent);
    }

    private sealed class FakeReminderRepository(IEnumerable<AppointmentReminder>? seed = null)
        : IRepository<AppointmentReminder>
    {
        public List<AppointmentReminder> Items { get; } = seed?.ToList() ?? [];

        public Task AddAsync(AppointmentReminder entity, CancellationToken cancellationToken = default)
        {
            Items.Add(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(AppointmentReminder entity, CancellationToken cancellationToken = default)
        {
            Items.Remove(entity);
            return Task.CompletedTask;
        }

        public Task<AppointmentReminder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(x => x.Id == id));

        public Task<IReadOnlyList<AppointmentReminder>> ListAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AppointmentReminder>>(Items);

        public Task UpdateAsync(AppointmentReminder entity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<PagedResult<AppointmentReminder>> SearchAsync(
            Func<IQueryable<AppointmentReminder>, IQueryable<AppointmentReminder>>? queryShaper,
            int pageNumber,
            int pageSize,
            bool applyDefaultIdOrdering = true,
            CancellationToken cancellationToken = default)
        {
            IQueryable<AppointmentReminder> q = Items.AsQueryable();
            if (queryShaper is not null)
                q = queryShaper(q);
            var list = q.ToList();
            return Task.FromResult(new PagedResult<AppointmentReminder>
            {
                Items = list,
                TotalCount = list.Count,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }
    }
}
