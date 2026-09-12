using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.Commands.CallAdminClinicConsultTicket;
using RaphCare.Application.Features.Organization.Commands.CompleteAdminClinicConsultTicket;
using RaphCare.Application.Features.Organization.Commands.CreateAdminClinicConsultTicket;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Patients;
using Xunit;

namespace RaphCare.Application.Tests.Features.Organization.Commands.Consult;

public sealed class ConsultHandlerTests
{
    [Fact]
    public async Task CreateConsultTicketFromScheduledAppointmentAllocatesQueueCode()
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111301");
        var appointmentId = Guid.Parse("22222222-2222-2222-2222-222222222301");
        var patientId = Guid.Parse("33333333-3333-3333-3333-333333333301");
        var patient = new Patient { FirstName = "Amina", LastName = "K." };
        EntityId.SetId(patient, patientId);
        var appointment = new Appointment
        {
            ClinicId = clinicId,
            PatientId = patientId,
            ProviderId = Guid.NewGuid(),
            ScheduledStart = new DateTime(2026, 9, 12, 9, 0, 0, DateTimeKind.Utc),
            ScheduledEnd = new DateTime(2026, 9, 12, 9, 30, 0, DateTimeKind.Utc),
            Type = "InPerson",
            Status = "Scheduled"
        };
        EntityId.SetId(appointment, appointmentId);

        var tickets = new FakeRepository<ConsultTicket>();
        var handler = new CreateAdminClinicConsultTicketHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            tickets,
            new FakeRepository<Appointment>([appointment]),
            new FakeRepository<Patient>([patient]),
            new FakeClock(new DateTime(2026, 9, 12, 8, 50, 0, DateTimeKind.Utc)),
            new FakeUnitOfWork());

        var result = await handler.Handle(
            new CreateAdminClinicConsultTicketCommand
            {
                ClinicId = clinicId,
                AppointmentId = appointmentId
            },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Waiting", result!.Status);
        Assert.Equal(6, result.QueueCode.Length);
        Assert.Equal(appointmentId, result.AppointmentId);
        Assert.Equal("Amina K.", result.PatientName);
        Assert.Single(tickets.Items);
    }

    [Fact]
    public async Task CreateConsultTicketRejectsCancelledAppointment()
    {
        var clinicId = Guid.NewGuid();
        var appointment = new Appointment
        {
            ClinicId = clinicId,
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            ScheduledStart = DateTime.UtcNow,
            ScheduledEnd = DateTime.UtcNow.AddMinutes(30),
            Type = "InPerson",
            Status = "Scheduled",
            IsCancelled = true
        };
        var handler = new CreateAdminClinicConsultTicketHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            new FakeRepository<ConsultTicket>(),
            new FakeRepository<Appointment>([appointment]),
            new FakeRepository<Patient>(),
            new FakeClock(DateTime.UtcNow),
            new FakeUnitOfWork());

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.Handle(
                new CreateAdminClinicConsultTicketCommand
                {
                    ClinicId = clinicId,
                    AppointmentId = appointment.Id
                },
                CancellationToken.None));
    }

    [Fact]
    public async Task CallThenCompleteConsultTicketUpdatesStatus()
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111302");
        var now = new DateTime(2026, 9, 12, 11, 0, 0, DateTimeKind.Utc);
        var ticket = new ConsultTicket
        {
            ClinicId = clinicId,
            AppointmentId = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            QueueCode = "QR12AB",
            Status = "Waiting",
            ArrivedAt = now.AddMinutes(-15)
        };
        var tickets = new FakeRepository<ConsultTicket>([ticket]);
        var callHandler = new CallAdminClinicConsultTicketHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            tickets,
            new FakeRepository<Appointment>(),
            new FakeRepository<Patient>(),
            new FakeClock(now),
            new FakeUnitOfWork());

        var called = await callHandler.Handle(
            new CallAdminClinicConsultTicketCommand { ClinicId = clinicId, TicketId = ticket.Id },
            CancellationToken.None);

        Assert.Equal("Called", called!.Status);
        Assert.Equal(now, ticket.CalledAt);

        var completeHandler = new CompleteAdminClinicConsultTicketHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            tickets,
            new FakeRepository<Appointment>(),
            new FakeRepository<Patient>(),
            new FakeClock(now.AddMinutes(5)),
            new FakeUnitOfWork());

        var completed = await completeHandler.Handle(
            new CompleteAdminClinicConsultTicketCommand { ClinicId = clinicId, TicketId = ticket.Id },
            CancellationToken.None);

        Assert.Equal("Completed", completed!.Status);
        Assert.NotNull(ticket.CompletedAt);
    }

    [Fact]
    public async Task CallConsultTicketRejectsWhenNotWaiting()
    {
        var clinicId = Guid.NewGuid();
        var ticket = new ConsultTicket
        {
            ClinicId = clinicId,
            QueueCode = "ZZ99AA",
            Status = "Called",
            ArrivedAt = DateTime.UtcNow,
            CalledAt = DateTime.UtcNow
        };
        var handler = new CallAdminClinicConsultTicketHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            new FakeRepository<ConsultTicket>([ticket]),
            new FakeRepository<Appointment>(),
            new FakeRepository<Patient>(),
            new FakeClock(DateTime.UtcNow),
            new FakeUnitOfWork());

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.Handle(
                new CallAdminClinicConsultTicketCommand { ClinicId = clinicId, TicketId = ticket.Id },
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
