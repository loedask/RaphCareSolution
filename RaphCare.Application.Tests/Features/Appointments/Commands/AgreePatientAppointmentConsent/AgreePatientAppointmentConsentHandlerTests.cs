using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Appointments.Commands.AgreePatientAppointmentConsent;
using RaphCare.Application.Features.Appointments.Queries.GetPatientAppointmentConsent;
using RaphCare.Domain.Clinical;
using Xunit;

namespace RaphCare.Application.Tests.Features.Appointments.Commands.AgreePatientAppointmentConsent;

public sealed class AgreePatientAppointmentConsentHandlerTests
{
    [Fact]
    public async Task AgreeCreatesAppointmentConsent()
    {
        var patientId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa01");
        var userId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01");
        var clinicId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccc01");
        var appointmentId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd01");
        var now = new DateTime(2026, 9, 12, 10, 0, 0, DateTimeKind.Utc);

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
        EntityId.SetId(appointment, appointmentId);

        var template = new ClinicConsentTemplate
        {
            ClinicId = clinicId,
            Title = "Visit consent",
            Body = "I agree to treatment.",
            IsActive = true
        };

        var consents = new FakeRepository<AppointmentConsent>();
        var handler = new AgreePatientAppointmentConsentHandler(
            new FakeCurrentUser(userId, patientId),
            new FakeClock(now),
            new FakeRepository<Appointment>([appointment]),
            consents,
            new FakeRepository<ClinicConsentTemplate>([template]),
            new FakeUnitOfWork());

        var result = await handler.Handle(
            new AgreePatientAppointmentConsentCommand { AppointmentId = appointmentId },
            CancellationToken.None);

        Assert.True(result.AlreadySigned);
        Assert.False(result.NeedsConsent);
        Assert.Equal(now, result.SignedAt);
        Assert.Equal("Visit consent", result.Title);
        Assert.Single(consents.Items);
        Assert.Equal(appointmentId, consents.Items[0].AppointmentId);
        Assert.Equal(template.Id, consents.Items[0].TemplateId);
        Assert.Equal(userId, consents.Items[0].SignedByApplicationUserId);
    }

    [Fact]
    public async Task AgreeRejectsInactiveTemplateForNewAgree()
    {
        var patientId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var clinicId = Guid.NewGuid();
        var appointment = new Appointment
        {
            ClinicId = clinicId,
            PatientId = patientId,
            ProviderId = Guid.NewGuid(),
            ScheduledStart = DateTime.UtcNow.AddHours(1),
            ScheduledEnd = DateTime.UtcNow.AddHours(1).AddMinutes(30),
            Type = "InPerson",
            Status = "Scheduled"
        };
        var inactive = new ClinicConsentTemplate
        {
            ClinicId = clinicId,
            Title = "Old form",
            Body = "Inactive",
            IsActive = false
        };

        var handler = new AgreePatientAppointmentConsentHandler(
            new FakeCurrentUser(userId, patientId),
            new FakeClock(DateTime.UtcNow),
            new FakeRepository<Appointment>([appointment]),
            new FakeRepository<AppointmentConsent>(),
            new FakeRepository<ClinicConsentTemplate>([inactive]),
            new FakeUnitOfWork());

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.Handle(
                new AgreePatientAppointmentConsentCommand { AppointmentId = appointment.Id },
                CancellationToken.None));
    }

    [Fact]
    public async Task GetReturnsAlreadySignedAfterAgree()
    {
        var patientId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa02");
        var userId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02");
        var clinicId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccc02");
        var appointmentId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd02");
        var now = new DateTime(2026, 9, 12, 11, 0, 0, DateTimeKind.Utc);

        var appointment = new Appointment
        {
            ClinicId = clinicId,
            PatientId = patientId,
            ProviderId = Guid.NewGuid(),
            ScheduledStart = now.AddHours(3),
            ScheduledEnd = now.AddHours(3).AddMinutes(30),
            Type = "InPerson",
            Status = "Scheduled"
        };
        EntityId.SetId(appointment, appointmentId);

        var template = new ClinicConsentTemplate
        {
            ClinicId = clinicId,
            Title = "Practice consent",
            Body = "Please agree before your visit.",
            IsActive = true
        };

        var appointments = new FakeRepository<Appointment>([appointment]);
        var consents = new FakeRepository<AppointmentConsent>();
        var templates = new FakeRepository<ClinicConsentTemplate>([template]);
        var currentUser = new FakeCurrentUser(userId, patientId);
        var clock = new FakeClock(now);

        var agreeHandler = new AgreePatientAppointmentConsentHandler(
            currentUser,
            clock,
            appointments,
            consents,
            templates,
            new FakeUnitOfWork());

        await agreeHandler.Handle(
            new AgreePatientAppointmentConsentCommand { AppointmentId = appointmentId },
            CancellationToken.None);

        var getHandler = new GetPatientAppointmentConsentHandler(
            currentUser,
            appointments,
            consents,
            templates);

        var result = await getHandler.Handle(
            new GetPatientAppointmentConsentQuery { AppointmentId = appointmentId },
            CancellationToken.None);

        Assert.True(result.AlreadySigned);
        Assert.False(result.NeedsConsent);
        Assert.Equal(now, result.SignedAt);
        Assert.Equal("Practice consent", result.Title);
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

file sealed class FakeCurrentUser(Guid userId, Guid patientId) : ICurrentUserService
{
    public string? UserId { get; } = userId.ToString();
    public Guid? CurrentUserId { get; } = userId;
    public string? UserName { get; } = "patient@example.com";
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
