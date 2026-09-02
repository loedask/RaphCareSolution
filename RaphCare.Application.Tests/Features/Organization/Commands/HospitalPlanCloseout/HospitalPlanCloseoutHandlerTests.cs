using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.Commands.CompleteAdminClinicLabOrder;
using RaphCare.Application.Features.Organization.Commands.DraftAdminClinicDischargeSummary;
using RaphCare.Application.Features.PatientNotifications.Commands.CreatePatientInAppNotification;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Identity;
using RaphCare.Domain.Organization;
using Xunit;

namespace RaphCare.Application.Tests.Features.Organization.Commands.HospitalPlanCloseout;

public sealed class DraftAdminClinicDischargeSummaryHandlerTests
{
    [Fact]
    public async Task HandleBuildsPromptFromReasonAndWardNotes()
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111401");
        var admission = new InpatientAdmission
        {
            ClinicId = clinicId,
            PatientId = Guid.NewGuid(),
            BedId = Guid.NewGuid(),
            AdmittedAt = new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc),
            Status = "Admitted",
            Reason = "Malaria"
        };
        var note = new InpatientObservation
        {
            AdmissionId = admission.Id,
            RecordedAt = admission.AdmittedAt.AddHours(6),
            Note = "Fever down. Drinking fluids.",
            HeartRate = 88
        };
        var ai = new CapturingAiService("Draft: malaria treated. Continue tablets.");
        var handler = new DraftAdminClinicDischargeSummaryHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            new FakeRoles([RaphCareRoles.Administrator]),
            new FakeRepository<InpatientAdmission>([admission]),
            new FakeRepository<InpatientObservation>([note]),
            ai);

        var result = await handler.Handle(
            new DraftAdminClinicDischargeSummaryCommand
            {
                ClinicId = clinicId,
                AdmissionId = admission.Id
            },
            CancellationToken.None);

        Assert.Equal("Draft: malaria treated. Continue tablets.", result!.DraftText);
        Assert.Contains("Malaria", ai.LastInput, StringComparison.Ordinal);
        Assert.Contains("Fever down", ai.LastInput, StringComparison.Ordinal);
        Assert.Contains("HR 88", ai.LastInput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleRejectsNonAdministrator()
    {
        var handler = new DraftAdminClinicDischargeSummaryHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            new FakeRoles([RaphCareRoles.Nurse]),
            new FakeRepository<InpatientAdmission>(),
            new FakeRepository<InpatientObservation>(),
            new CapturingAiService("x"));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(
                new DraftAdminClinicDischargeSummaryCommand
                {
                    ClinicId = Guid.NewGuid(),
                    AdmissionId = Guid.NewGuid()
                },
                CancellationToken.None));
    }
}

public sealed class CompleteAdminClinicLabOrderHandlerTests
{
    [Fact]
    public async Task HandleNotifiesPatientWhenLabResultIsReady()
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111402");
        var patientId = Guid.Parse("33333333-3333-3333-3333-333333333402");
        var visitId = Guid.Parse("44444444-4444-4444-4444-444444444402");
        var labRequestId = Guid.Parse("55555555-5555-5555-5555-555555555502");

        var visit = new Visit
        {
            ClinicId = clinicId,
            PatientId = patientId,
            VisitStart = DateTime.UtcNow.AddHours(-2),
            VisitType = "InPerson",
            Status = "InProgress"
        };
        EntityId.SetId(visit, visitId);

        var labRequest = new LabRequest
        {
            VisitId = visitId,
            TestName = "Malaria smear",
            Status = "Pending",
            RequestedAt = DateTime.UtcNow.AddHours(-1),
            PickupCode = "LAB123"
        };
        EntityId.SetId(labRequest, labRequestId);

        var clinic = new Clinic { Name = "Demo Clinic" };
        EntityId.SetId(clinic, clinicId);

        var mediator = new CapturingMediator();
        var handler = new CompleteAdminClinicLabOrderHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            new FakeRoles([RaphCareRoles.LabTechnician]),
            new FakeRepository<Visit>([visit]),
            new FakeRepository<LabRequest>([labRequest]),
            new FakeRepository<LabResult>(),
            new FakeRepository<Clinic>([clinic]),
            mediator,
            new FakeClock(DateTime.UtcNow),
            new FakeUnitOfWork());

        var result = await handler.Handle(
            new CompleteAdminClinicLabOrderCommand
            {
                ClinicId = clinicId,
                LabRequestId = labRequestId,
                ResultValue = "Negative"
            },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Completed", labRequest.Status);
        var notice = Assert.IsType<CreatePatientInAppNotificationCommand>(mediator.LastRequest);
        Assert.Equal(patientId, notice.PatientId);
        Assert.Equal("Your lab result is ready", notice.Title);
        Assert.Equal("lab", notice.Type);
        Assert.Contains("Malaria smear", notice.Body, StringComparison.Ordinal);
        Assert.True(notice.SendPush);
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

file sealed class CapturingAiService(string reply) : IAIService
{
    public string LastInput { get; private set; } = string.Empty;

    public Task<string> GenerateSummaryAsync(string input, CancellationToken cancellationToken = default)
    {
        LastInput = input;
        return Task.FromResult(reply);
    }

    public Task<string> GeneratePatientAssistantReplyAsync(string userMessage, CancellationToken cancellationToken = default) =>
        Task.FromResult(reply);
}

file sealed class CapturingMediator : IMediator
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

file sealed class FakeRoles(IReadOnlyList<string> roles) : IUserRoleAssignmentService
{
    public Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(roles);

    public Task AssignRoleIfMissingAsync(Guid userId, string roleName, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task RemoveRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task SetStaffJobRoleAsync(Guid userId, string jobRole, CancellationToken cancellationToken = default) =>
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
