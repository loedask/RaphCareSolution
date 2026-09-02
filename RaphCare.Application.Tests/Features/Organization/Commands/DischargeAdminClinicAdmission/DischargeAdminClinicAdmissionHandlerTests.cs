using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.Commands.CreateAdminClinicAdmissionObservation;
using RaphCare.Application.Features.Organization.Commands.DischargeAdminClinicAdmission;
using RaphCare.Domain.Billing;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Identity;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Patients;
using Xunit;

namespace RaphCare.Application.Tests.Features.Organization.Commands.DischargeAdminClinicAdmission;

public sealed class DischargeAdminClinicAdmissionHandlerTests
{
    [Fact]
    public async Task HandleCreatesPaidInvoiceWithBedNightsAndWritesDischargeSummary()
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111101");
        var userId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var now = new DateTime(2026, 9, 2, 12, 0, 0, DateTimeKind.Utc);
        var admission = new InpatientAdmission
        {
            ClinicId = clinicId,
            PatientId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            BedId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            AdmittedAt = now.AddHours(-36),
            Status = "Admitted",
            Reason = "Malaria"
        };
        var bed = new Bed { RoomId = Guid.Parse("55555555-5555-5555-5555-555555555555"), Label = "B1", Status = "Occupied" };
        EntityId.SetId(bed, admission.BedId);
        var patient = new Patient { FirstName = "Amina", LastName = "K." };
        EntityId.SetId(patient, admission.PatientId);

        var admissions = new FakeRepository<InpatientAdmission>([admission]);
        var beds = new FakeRepository<Bed>([bed]);
        var invoices = new FakeRepository<Invoice>();
        var lines = new FakeRepository<InvoiceLineItem>();

        var handler = new DischargeAdminClinicAdmissionHandler(
            new FakeCurrentUser(userId),
            new FakeMembership(true),
            new FakeRoles([RaphCareRoles.Administrator]),
            new FakePatientAccess(true),
            admissions,
            beds,
            new FakeRepository<Room>(),
            new FakeRepository<Ward>(),
            new FakeRepository<Facility>(),
            new FakeRepository<Patient>([patient]),
            invoices,
            lines,
            new FakeRepository<Appointment>(),
            new FakeRepository<Provider>(),
            new FakeRepository<ProviderSchedule>(),
            new FakeRepository<Clinic>(),
            new FakeClock(now),
            new FakeUnitOfWork());

        var result = await handler.Handle(
            new DischargeAdminClinicAdmissionCommand
            {
                ClinicId = clinicId,
                AdmissionId = admission.Id,
                DischargeSummary = "Treated for malaria. Continue the tablets.",
                NightlyBedRate = 100,
                ExtraAmount = 50,
                ExtraDescription = "Lab",
                MarkPaid = true,
                Currency = "zar"
            },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Discharged", admission.Status);
        Assert.Equal("Available", bed.Status);
        Assert.Equal("Treated for malaria. Continue the tablets.", admission.DischargeSummary);
        Assert.Equal(2, admission.CountChargeableBedNights(now));
        Assert.NotNull(admission.InvoiceId);
        var invoice = invoices.Items.Single();
        Assert.Equal(admission.Id, invoice.AdmissionId);
        Assert.Equal(250, invoice.Amount);
        Assert.Equal("ZAR", invoice.Currency);
        Assert.Equal("Paid", invoice.Status);
        Assert.Equal("Cash", invoice.PaymentMethod);
        Assert.Equal(2, lines.Items.Count);
        Assert.Equal(250, result!.InvoiceAmount);
        Assert.Equal("Paid", result.InvoiceStatus);
        Assert.Equal(2, result.BedNights);
        Assert.Null(result.ReturnAppointmentId);
    }

    [Fact]
    public async Task HandleDoesNotCreateInvoiceWhenRatesAreZero()
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111101");
        var now = new DateTime(2026, 9, 2, 12, 0, 0, DateTimeKind.Utc);
        var admission = new InpatientAdmission
        {
            ClinicId = clinicId,
            PatientId = Guid.NewGuid(),
            BedId = Guid.NewGuid(),
            AdmittedAt = now.AddHours(-4),
            Status = "Admitted"
        };
        var bed = new Bed { Status = "Occupied" };
        EntityId.SetId(bed, admission.BedId);
        var invoices = new FakeRepository<Invoice>();

        var handler = new DischargeAdminClinicAdmissionHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            new FakeRoles([RaphCareRoles.Administrator]),
            new FakePatientAccess(true),
            new FakeRepository<InpatientAdmission>([admission]),
            new FakeRepository<Bed>([bed]),
            new FakeRepository<Room>(),
            new FakeRepository<Ward>(),
            new FakeRepository<Facility>(),
            new FakeRepository<Patient>(),
            invoices,
            new FakeRepository<InvoiceLineItem>(),
            new FakeRepository<Appointment>(),
            new FakeRepository<Provider>(),
            new FakeRepository<ProviderSchedule>(),
            new FakeRepository<Clinic>(),
            new FakeClock(now),
            new FakeUnitOfWork());

        var result = await handler.Handle(
            new DischargeAdminClinicAdmissionCommand { ClinicId = clinicId, AdmissionId = admission.Id },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Null(admission.InvoiceId);
        Assert.Empty(invoices.Items);
    }

    [Fact]
    public async Task HandleBooksReturnVisitWhenRequestedAtDischarge()
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111101");
        var now = new DateTime(2026, 9, 2, 12, 0, 0, DateTimeKind.Utc);
        var patientId = Guid.Parse("33333333-3333-3333-3333-333333333301");
        var providerId = Guid.Parse("66666666-6666-6666-6666-666666666601");
        var admission = new InpatientAdmission
        {
            ClinicId = clinicId,
            PatientId = patientId,
            BedId = Guid.Parse("44444444-4444-4444-4444-444444444401"),
            AdmittedAt = now.AddDays(-2),
            Status = "Admitted"
        };
        var bed = new Bed { Status = "Occupied" };
        EntityId.SetId(bed, admission.BedId);
        var patient = new Patient { FirstName = "Amina", LastName = "K." };
        EntityId.SetId(patient, patientId);
        var provider = new Provider
        {
            ClinicId = clinicId,
            ApplicationUserId = Guid.NewGuid(),
            IsActive = true,
            LicenseNumber = "MD-1"
        };
        EntityId.SetId(provider, providerId);

        var appointments = new FakeRepository<Appointment>();
        var start = now.AddDays(7);
        var end = start.AddMinutes(30);

        var handler = new DischargeAdminClinicAdmissionHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            new FakeRoles([RaphCareRoles.Administrator]),
            new FakePatientAccess(true),
            new FakeRepository<InpatientAdmission>([admission]),
            new FakeRepository<Bed>([bed]),
            new FakeRepository<Room>(),
            new FakeRepository<Ward>(),
            new FakeRepository<Facility>(),
            new FakeRepository<Patient>([patient]),
            new FakeRepository<Invoice>(),
            new FakeRepository<InvoiceLineItem>(),
            appointments,
            new FakeRepository<Provider>([provider]),
            new FakeRepository<ProviderSchedule>(),
            new FakeRepository<Clinic>(),
            new FakeClock(now),
            new FakeUnitOfWork());

        var result = await handler.Handle(
            new DischargeAdminClinicAdmissionCommand
            {
                ClinicId = clinicId,
                AdmissionId = admission.Id,
                BookReturnVisit = true,
                ReturnProviderId = providerId,
                ReturnScheduledStart = start,
                ReturnScheduledEnd = end,
                ReturnAppointmentType = "InPerson",
                ReturnReason = "Wound check"
            },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Discharged", admission.Status);
        Assert.Equal("Available", bed.Status);
        var booked = Assert.Single(appointments.Items);
        Assert.Equal(patientId, booked.PatientId);
        Assert.Equal(providerId, booked.ProviderId);
        Assert.Equal("Scheduled", booked.Status);
        Assert.Equal("Wound check", booked.Reason);
        Assert.Equal(booked.Id, result!.ReturnAppointmentId);
        Assert.Equal(start, result.ReturnAppointmentStart);
        Assert.Equal(end, result.ReturnAppointmentEnd);
    }

    [Fact]
    public async Task HandleRejectsReturnVisitWhenProviderConflicts()
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111101");
        var now = new DateTime(2026, 9, 2, 12, 0, 0, DateTimeKind.Utc);
        var patientId = Guid.Parse("33333333-3333-3333-3333-333333333302");
        var providerId = Guid.Parse("66666666-6666-6666-6666-666666666602");
        var admission = new InpatientAdmission
        {
            ClinicId = clinicId,
            PatientId = patientId,
            BedId = Guid.Parse("44444444-4444-4444-4444-444444444402"),
            AdmittedAt = now.AddDays(-1),
            Status = "Admitted"
        };
        var bed = new Bed { Status = "Occupied" };
        EntityId.SetId(bed, admission.BedId);
        var patient = new Patient { FirstName = "Jean", LastName = "M." };
        EntityId.SetId(patient, patientId);
        var provider = new Provider
        {
            ClinicId = clinicId,
            ApplicationUserId = Guid.NewGuid(),
            IsActive = true,
            LicenseNumber = "MD-2"
        };
        EntityId.SetId(provider, providerId);

        var start = now.AddDays(3);
        var end = start.AddMinutes(30);
        var existing = new Appointment
        {
            ClinicId = clinicId,
            PatientId = Guid.NewGuid(),
            ProviderId = providerId,
            ScheduledStart = start.AddMinutes(-10),
            ScheduledEnd = end.AddMinutes(10),
            Type = "InPerson",
            Status = "Scheduled"
        };

        var handler = new DischargeAdminClinicAdmissionHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            new FakeRoles([RaphCareRoles.Administrator]),
            new FakePatientAccess(true),
            new FakeRepository<InpatientAdmission>([admission]),
            new FakeRepository<Bed>([bed]),
            new FakeRepository<Room>(),
            new FakeRepository<Ward>(),
            new FakeRepository<Facility>(),
            new FakeRepository<Patient>([patient]),
            new FakeRepository<Invoice>(),
            new FakeRepository<InvoiceLineItem>(),
            new FakeRepository<Appointment>([existing]),
            new FakeRepository<Provider>([provider]),
            new FakeRepository<ProviderSchedule>(),
            new FakeRepository<Clinic>(),
            new FakeClock(now),
            new FakeUnitOfWork());

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.Handle(
                new DischargeAdminClinicAdmissionCommand
                {
                    ClinicId = clinicId,
                    AdmissionId = admission.Id,
                    BookReturnVisit = true,
                    ReturnProviderId = providerId,
                    ReturnScheduledStart = start,
                    ReturnScheduledEnd = end
                },
                CancellationToken.None));

        Assert.Equal("Admitted", admission.Status);
        Assert.Equal("Occupied", bed.Status);
    }
}

public sealed class CreateAdminClinicAdmissionObservationHandlerTests
{
    [Fact]
    public async Task HandleAllowsNurseToAddNoteOnActiveStay()
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111101");
        var admission = new InpatientAdmission
        {
            ClinicId = clinicId,
            PatientId = Guid.NewGuid(),
            BedId = Guid.NewGuid(),
            AdmittedAt = DateTime.UtcNow.AddDays(-1),
            Status = "Admitted"
        };
        var observations = new FakeRepository<InpatientObservation>();
        var handler = new CreateAdminClinicAdmissionObservationHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            new FakeRoles([RaphCareRoles.Nurse]),
            new FakeRepository<InpatientAdmission>([admission]),
            observations,
            new FakeClock(DateTime.UtcNow),
            new FakeUnitOfWork());

        var result = await handler.Handle(
            new CreateAdminClinicAdmissionObservationCommand
            {
                ClinicId = clinicId,
                AdmissionId = admission.Id,
                Note = "Slept well. Drinking fluids.",
                HeartRate = 78
            },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(observations.Items);
        Assert.Equal("Slept well. Drinking fluids.", result!.Note);
        Assert.Equal(78, result.HeartRate);
    }

    [Fact]
    public async Task HandleRejectsPharmacistWardNote()
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111101");
        var handler = new CreateAdminClinicAdmissionObservationHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            new FakeRoles([RaphCareRoles.Pharmacist]),
            new FakeRepository<InpatientAdmission>(),
            new FakeRepository<InpatientObservation>(),
            new FakeClock(DateTime.UtcNow),
            new FakeUnitOfWork());

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(
                new CreateAdminClinicAdmissionObservationCommand
                {
                    ClinicId = clinicId,
                    AdmissionId = Guid.NewGuid(),
                    Note = "Should not save"
                },
                CancellationToken.None));
    }

    [Fact]
    public async Task HandleRejectsNoteAfterDischarge()
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111101");
        var admission = new InpatientAdmission
        {
            ClinicId = clinicId,
            PatientId = Guid.NewGuid(),
            BedId = Guid.NewGuid(),
            AdmittedAt = DateTime.UtcNow.AddDays(-2),
            Status = "Discharged",
            DischargedAt = DateTime.UtcNow
        };
        var handler = new CreateAdminClinicAdmissionObservationHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            new FakeRoles([RaphCareRoles.Nurse]),
            new FakeRepository<InpatientAdmission>([admission]),
            new FakeRepository<InpatientObservation>(),
            new FakeClock(DateTime.UtcNow),
            new FakeUnitOfWork());

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.Handle(
                new CreateAdminClinicAdmissionObservationCommand
                {
                    ClinicId = clinicId,
                    AdmissionId = admission.Id,
                    Note = "Too late"
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
