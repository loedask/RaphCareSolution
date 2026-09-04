using FluentValidation;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.MentalHealth;
using RaphCare.Application.Features.MentalHealth.Commands.CreateMentalHealthAssessment;
using RaphCare.Domain.MentalHealth;
using RaphCare.Domain.Patients;
using Xunit;

namespace RaphCare.Application.Tests.Features.MentalHealth.Commands.CreateMentalHealthAssessment;

public sealed class CreateMentalHealthAssessmentHandlerTests
{
    [Fact]
    public async Task CreatePhq9PersistsNineAnswersAndMildSeverityForScoreEight()
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111401");
        var patientId = Guid.Parse("33333333-3333-3333-3333-333333333401");
        var patient = new Patient { FirstName = "Amina", LastName = "K." };
        EntityId.SetId(patient, patientId);
        var assessments = new FakeRepository<MentalHealthAssessment>();
        var now = new DateTime(2026, 9, 4, 12, 0, 0, DateTimeKind.Utc);
        var handler = new CreateMentalHealthAssessmentHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            new FakePatientAccess(true),
            new FakeRepository<Patient>([patient]),
            assessments,
            new FakeClock(now),
            new FakeUnitOfWork());

        // Scores that sum to 8 → Mild
        int[] itemScores = [1, 1, 1, 1, 1, 1, 1, 1, 0];
        var answers = itemScores
            .Select((score, i) => new CreateMentalHealthAssessmentAnswer
            {
                Order = i + 1,
                NumericScore = score
            })
            .ToList();

        var result = await handler.Handle(
            new CreateMentalHealthAssessmentCommand
            {
                ClinicId = clinicId,
                PatientId = patientId,
                AssessmentType = Phq9Instrument.AssessmentType,
                Answers = answers
            },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(Phq9Instrument.AssessmentType, result!.AssessmentType);
        Assert.Equal(8, result.TotalScore);
        Assert.Equal("Mild", result.SeverityLevel);
        Assert.Equal(now, result.AssessedAt);
        Assert.Equal(9, result.Items.Count);
        Assert.Equal(9, assessments.Items.Single().Questions.Count);
        Assert.Equal(9, assessments.Items.Single().Responses.Count);
        Assert.Equal("Several days", result.Items[0].ResponseValue);
        Assert.Contains("PHQ-9", result.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreatePhq9UsesSevereBandForHighTotal()
    {
        var clinicId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var patient = new Patient { FirstName = "Paul", LastName = "N." };
        EntityId.SetId(patient, patientId);
        var handler = new CreateMentalHealthAssessmentHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            new FakePatientAccess(true),
            new FakeRepository<Patient>([patient]),
            new FakeRepository<MentalHealthAssessment>(),
            new FakeClock(DateTime.UtcNow),
            new FakeUnitOfWork());

        var answers = Enumerable.Range(1, 9)
            .Select(order => new CreateMentalHealthAssessmentAnswer { Order = order, NumericScore = 3 })
            .ToList();

        var result = await handler.Handle(
            new CreateMentalHealthAssessmentCommand
            {
                ClinicId = clinicId,
                PatientId = patientId,
                Answers = answers
            },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(27, result!.TotalScore);
        Assert.Equal("Severe", result.SeverityLevel);
    }

    [Fact]
    public async Task CreateRejectsWhenPatientLacksClinicAccess()
    {
        var patientId = Guid.NewGuid();
        var patient = new Patient { FirstName = "No", LastName = "Access" };
        EntityId.SetId(patient, patientId);
        var handler = new CreateMentalHealthAssessmentHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            new FakePatientAccess(false),
            new FakeRepository<Patient>([patient]),
            new FakeRepository<MentalHealthAssessment>(),
            new FakeClock(DateTime.UtcNow),
            new FakeUnitOfWork());

        var answers = Enumerable.Range(1, 9)
            .Select(order => new CreateMentalHealthAssessmentAnswer { Order = order, NumericScore = 0 })
            .ToList();

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.Handle(
                new CreateMentalHealthAssessmentCommand
                {
                    ClinicId = Guid.NewGuid(),
                    PatientId = patientId,
                    Answers = answers
                },
                CancellationToken.None));
    }

    [Fact]
    public async Task CreateRejectsWhenCallerIsNotClinicStaff()
    {
        var handler = new CreateMentalHealthAssessmentHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(false),
            new FakePatientAccess(true),
            new FakeRepository<Patient>(),
            new FakeRepository<MentalHealthAssessment>(),
            new FakeClock(DateTime.UtcNow),
            new FakeUnitOfWork());

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(
                new CreateMentalHealthAssessmentCommand
                {
                    ClinicId = Guid.NewGuid(),
                    PatientId = Guid.NewGuid(),
                    Answers = Enumerable.Range(1, 9)
                        .Select(o => new CreateMentalHealthAssessmentAnswer { Order = o, NumericScore = 0 })
                        .ToList()
                },
                CancellationToken.None));
    }

    [Fact]
    public void ValidatorRejectsWrongAnswerCount()
    {
        var validator = new CreateMentalHealthAssessmentValidator();
        var result = validator.Validate(new CreateMentalHealthAssessmentCommand
        {
            ClinicId = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            AssessmentType = Phq9Instrument.AssessmentType,
            Answers =
            [
                new CreateMentalHealthAssessmentAnswer { Order = 1, NumericScore = 0 }
            ]
        });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains("Answers", StringComparison.Ordinal));
    }

    [Fact]
    public void SeverityBandsMatchPhq9Cutoffs()
    {
        Assert.Equal("Minimal", Phq9Instrument.SeverityForTotal(4));
        Assert.Equal("Mild", Phq9Instrument.SeverityForTotal(5));
        Assert.Equal("Moderate", Phq9Instrument.SeverityForTotal(14));
        Assert.Equal("Moderately severe", Phq9Instrument.SeverityForTotal(15));
        Assert.Equal("Severe", Phq9Instrument.SeverityForTotal(20));
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
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
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

file sealed class FakePatientAccess(bool hasAccess) : IPatientClinicAccessService
{
    public Task<bool> HasClinicAccessAsync(Guid patientId, Guid clinicId, CancellationToken ct) =>
        Task.FromResult(hasAccess);

    public Task EnsureClinicAccessAsync(Guid patientId, Guid clinicId, CancellationToken ct) =>
        hasAccess ? Task.CompletedTask : throw new ForbiddenAccessException("No access.");

    public Task GrantEncounterAccessAsync(Guid patientId, Guid clinicId, CancellationToken ct) =>
        Task.CompletedTask;

    public Task<Guid[]> GetAccessibleClinicIdsAsync(Guid patientId, CancellationToken ct) =>
        Task.FromResult(Array.Empty<Guid>());

    public Task GrantManualAccessAsync(Guid patientId, Guid clinicId, string? notes, CancellationToken ct) =>
        Task.CompletedTask;

    public Task RevokeClinicAccessAsync(Guid patientId, Guid clinicId, CancellationToken ct) =>
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
