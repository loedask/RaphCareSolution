using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Appointments;
using RaphCare.Application.Features.Appointments.Commands.ProcessDueAppointmentReminders;
using RaphCare.Application.Features.PatientNotifications.Commands.CreatePatientInAppNotification;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Organization;
using Xunit;

namespace RaphCare.Application.Tests.Features.Appointments.Commands.ProcessDueAppointmentReminders;

public sealed class ProcessDueAppointmentRemindersHandlerTests
{
    [Fact]
    public async Task HandleSendsInAppNoticeAndMarksReminderSent()
    {
        var now = new DateTime(2026, 9, 12, 12, 0, 0, DateTimeKind.Utc);
        var appointmentId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1");
        var patientId = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc1");
        var clinicId = Guid.Parse("dddddddd-dddd-dddd-dddd-ddddddddddd1");

        var appointment = new Appointment
        {
            ClinicId = clinicId,
            PatientId = patientId,
            ProviderId = Guid.NewGuid(),
            ScheduledStart = now.AddHours(2),
            ScheduledEnd = now.AddHours(2).AddMinutes(30),
            Type = "InPerson",
            Status = "Scheduled"
        };
        SetId(appointment, appointmentId);

        var reminder = new AppointmentReminder
        {
            AppointmentId = appointmentId,
            ReminderTime = now.AddMinutes(-1),
            Channel = AppointmentReminderChannels.InApp,
            Sent = false
        };

        var clinic = new Clinic { Name = "Demo Practice" };
        SetId(clinic, clinicId);

        var mediator = new CapturingMediator();
        var reminderRepo = new FakeRepository<AppointmentReminder>([reminder]);
        var handler = new ProcessDueAppointmentRemindersHandler(
            reminderRepo,
            new FakeRepository<Appointment>([appointment]),
            new FakeRepository<Clinic>([clinic]),
            new FakeClock(now),
            mediator,
            new FakeUnitOfWork());

        var sent = await handler.Handle(new ProcessDueAppointmentRemindersCommand(), CancellationToken.None);

        Assert.Equal(1, sent);
        Assert.True(reminder.Sent);
        var notice = Assert.IsType<CreatePatientInAppNotificationCommand>(mediator.LastRequest);
        Assert.Equal(patientId, notice.PatientId);
        Assert.Equal("appointment", notice.Type);
        Assert.Equal("Upcoming appointment", notice.Title);
        Assert.True(notice.SendPush);
    }

    [Fact]
    public async Task HandleDoesNotNotifyWhenAppointmentCancelledButMarksSent()
    {
        var now = new DateTime(2026, 9, 12, 12, 0, 0, DateTimeKind.Utc);
        var appointmentId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2");
        var appointment = new Appointment
        {
            ClinicId = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            ScheduledStart = now.AddHours(2),
            ScheduledEnd = now.AddHours(3),
            Type = "InPerson",
            Status = "Cancelled",
            IsCancelled = true
        };
        SetId(appointment, appointmentId);

        var reminder = new AppointmentReminder
        {
            AppointmentId = appointmentId,
            ReminderTime = now.AddMinutes(-5),
            Channel = AppointmentReminderChannels.InApp,
            Sent = false
        };

        var mediator = new CapturingMediator();
        var handler = new ProcessDueAppointmentRemindersHandler(
            new FakeRepository<AppointmentReminder>([reminder]),
            new FakeRepository<Appointment>([appointment]),
            new FakeRepository<Clinic>(),
            new FakeClock(now),
            mediator,
            new FakeUnitOfWork());

        var sent = await handler.Handle(new ProcessDueAppointmentRemindersCommand(), CancellationToken.None);

        Assert.Equal(0, sent);
        Assert.True(reminder.Sent);
        Assert.Null(mediator.LastRequest);
    }

    private static void SetId<T>(T entity, Guid id) where T : class
    {
        var property = typeof(T).GetProperty("Id")
            ?? throw new InvalidOperationException($"{typeof(T).Name} has no Id.");
        property.SetValue(entity, id);
    }

    private sealed class CapturingMediator : IMediator
    {
        public object? LastRequest { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            if (typeof(TResponse) == typeof(Guid))
                return Task.FromResult((TResponse)(object)Guid.NewGuid());
            return Task.FromResult(default(TResponse)!);
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
        {
            LastRequest = request;
            return Task.CompletedTask;
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult<object?>(null);
        }

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamRequest<TResponse> request,
            CancellationToken cancellationToken = default) =>
            EmptyAsync<TResponse>();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
            EmptyAsync<object?>();

        public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification =>
            Task.CompletedTask;

        private static async IAsyncEnumerable<T> EmptyAsync<T>()
        {
            await Task.CompletedTask;
            yield break;
        }
    }

    private sealed class FakeClock(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    }

    private sealed class FakeRepository<T>(IEnumerable<T>? seed = null) : IRepository<T>
        where T : class
    {
        private readonly List<T> _items = seed?.ToList() ?? [];

        public Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            _items.Add(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            _items.Remove(entity);
            return Task.CompletedTask;
        }

        public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var prop = typeof(T).GetProperty("Id");
            return Task.FromResult(_items.FirstOrDefault(x => (Guid)prop!.GetValue(x)! == id));
        }

        public Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<T>>(_items);

        public Task UpdateAsync(T entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<PagedResult<T>> SearchAsync(
            Func<IQueryable<T>, IQueryable<T>>? queryShaper,
            int pageNumber,
            int pageSize,
            bool applyDefaultIdOrdering = true,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> q = _items.AsQueryable();
            if (queryShaper is not null)
                q = queryShaper(q);
            var list = q.ToList();
            return Task.FromResult(new PagedResult<T>
            {
                Items = list,
                TotalCount = list.Count,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }
    }
}
