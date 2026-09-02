using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.Commands.CallAdminClinicCasualtyTicket;
using RaphCare.Application.Features.Organization.Commands.CompleteAdminClinicCasualtyTicket;
using RaphCare.Application.Features.Organization.Commands.CreateAdminClinicCasualtyTicket;
using RaphCare.Application.Features.Organization.Commands.CreateAdminClinicTheatreCase;
using RaphCare.Application.Features.Organization.Commands.UpdateAdminClinicTheatreCaseStatus;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Patients;
using Xunit;

namespace RaphCare.Application.Tests.Features.Organization.Commands.CasualtyAndTheatre;

public sealed class CasualtyAndTheatreHandlerTests
{
    [Fact]
    public async Task CreateCasualtyTicketAllocatesQueueCodeAndStartsWaiting()
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111201");
        var tickets = new FakeRepository<CasualtyTicket>();
        var handler = new CreateAdminClinicCasualtyTicketHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            tickets,
            new FakeRepository<Patient>(),
            new FakeClock(new DateTime(2026, 9, 2, 10, 0, 0, DateTimeKind.Utc)),
            new FakeUnitOfWork());

        var result = await handler.Handle(
            new CreateAdminClinicCasualtyTicketCommand
            {
                ClinicId = clinicId,
                TriageLevel = "orange",
                ChiefComplaint = "Chest pain"
            },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Waiting", result!.Status);
        Assert.Equal("Orange", result.TriageLevel);
        Assert.Equal(6, result.QueueCode.Length);
        Assert.Single(tickets.Items);
    }

    [Fact]
    public async Task CallThenCompleteCasualtyTicketUpdatesStatus()
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111202");
        var now = new DateTime(2026, 9, 2, 11, 0, 0, DateTimeKind.Utc);
        var ticket = new CasualtyTicket
        {
            ClinicId = clinicId,
            QueueCode = "AB12CD",
            TriageLevel = "Yellow",
            Status = "Waiting",
            ArrivedAt = now.AddMinutes(-20)
        };
        var tickets = new FakeRepository<CasualtyTicket>([ticket]);
        var callHandler = new CallAdminClinicCasualtyTicketHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            tickets,
            new FakeRepository<Patient>(),
            new FakeClock(now),
            new FakeUnitOfWork());

        var called = await callHandler.Handle(
            new CallAdminClinicCasualtyTicketCommand { ClinicId = clinicId, TicketId = ticket.Id },
            CancellationToken.None);

        Assert.Equal("Called", called!.Status);
        Assert.Equal(now, ticket.CalledAt);

        var completeHandler = new CompleteAdminClinicCasualtyTicketHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            tickets,
            new FakeRepository<Patient>(),
            new FakeClock(now.AddMinutes(5)),
            new FakeUnitOfWork());

        var completed = await completeHandler.Handle(
            new CompleteAdminClinicCasualtyTicketCommand { ClinicId = clinicId, TicketId = ticket.Id },
            CancellationToken.None);

        Assert.Equal("Completed", completed!.Status);
        Assert.NotNull(ticket.CompletedAt);
    }

    [Fact]
    public async Task CallCasualtyTicketRejectsWhenNotWaiting()
    {
        var clinicId = Guid.NewGuid();
        var ticket = new CasualtyTicket
        {
            ClinicId = clinicId,
            QueueCode = "ZZ99AA",
            TriageLevel = "Green",
            Status = "Called",
            ArrivedAt = DateTime.UtcNow,
            CalledAt = DateTime.UtcNow
        };
        var handler = new CallAdminClinicCasualtyTicketHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            new FakeRepository<CasualtyTicket>([ticket]),
            new FakeRepository<Patient>(),
            new FakeClock(DateTime.UtcNow),
            new FakeUnitOfWork());

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.Handle(
                new CallAdminClinicCasualtyTicketCommand { ClinicId = clinicId, TicketId = ticket.Id },
                CancellationToken.None));
    }

    [Fact]
    public async Task CreateTheatreCaseSchedulesForPatient()
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111203");
        var patientId = Guid.Parse("33333333-3333-3333-3333-333333333203");
        var patient = new Patient { FirstName = "Jean", LastName = "M." };
        EntityId.SetId(patient, patientId);
        var cases = new FakeRepository<TheatreCase>();
        var handler = new CreateAdminClinicTheatreCaseHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            cases,
            new FakeRepository<Patient>([patient]),
            new FakeUnitOfWork());

        var start = new DateTime(2026, 9, 2, 14, 0, 0, DateTimeKind.Utc);
        var result = await handler.Handle(
            new CreateAdminClinicTheatreCaseCommand
            {
                ClinicId = clinicId,
                PatientId = patientId,
                ScheduledStart = start,
                ProcedureName = "Appendectomy",
                TheatreName = "OR 1",
                SurgeonName = "Dr. Okello"
            },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Scheduled", result!.Status);
        Assert.Equal("Appendectomy", result.ProcedureName);
        Assert.Equal("Jean M.", result.PatientName);
        Assert.Single(cases.Items);
    }

    [Fact]
    public async Task UpdateTheatreCaseStatusStartsThenCompletes()
    {
        var clinicId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var patient = new Patient { FirstName = "Amina", LastName = "K." };
        EntityId.SetId(patient, patientId);
        var theatreCase = new TheatreCase
        {
            ClinicId = clinicId,
            PatientId = patientId,
            ScheduledStart = DateTime.UtcNow,
            ProcedureName = "C-section",
            Status = "Scheduled"
        };
        var cases = new FakeRepository<TheatreCase>([theatreCase]);
        var handler = new UpdateAdminClinicTheatreCaseStatusHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            cases,
            new FakeRepository<Patient>([patient]),
            new FakeUnitOfWork());

        var started = await handler.Handle(
            new UpdateAdminClinicTheatreCaseStatusCommand
            {
                ClinicId = clinicId,
                CaseId = theatreCase.Id,
                Status = "InProgress"
            },
            CancellationToken.None);
        Assert.Equal("InProgress", started!.Status);

        var completed = await handler.Handle(
            new UpdateAdminClinicTheatreCaseStatusCommand
            {
                ClinicId = clinicId,
                CaseId = theatreCase.Id,
                Status = "Completed"
            },
            CancellationToken.None);
        Assert.Equal("Completed", completed!.Status);
    }

    [Fact]
    public async Task UpdateTheatreCaseStatusRejectsRestartAfterComplete()
    {
        var clinicId = Guid.NewGuid();
        var theatreCase = new TheatreCase
        {
            ClinicId = clinicId,
            PatientId = Guid.NewGuid(),
            ScheduledStart = DateTime.UtcNow,
            ProcedureName = "Done",
            Status = "Completed"
        };
        var handler = new UpdateAdminClinicTheatreCaseStatusHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            new FakeRepository<TheatreCase>([theatreCase]),
            new FakeRepository<Patient>(),
            new FakeUnitOfWork());

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.Handle(
                new UpdateAdminClinicTheatreCaseStatusCommand
                {
                    ClinicId = clinicId,
                    CaseId = theatreCase.Id,
                    Status = "InProgress"
                },
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
