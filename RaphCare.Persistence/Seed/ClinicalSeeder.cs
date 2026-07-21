using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RaphCare.Domain.MentalHealth;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Patients;
using RaphCare.Domain.Patients.Enums;
using RaphCare.Persistence;

namespace RaphCare.Persistence.Seed;

/// <summary>
/// Seeds clinical bounded-context data: clinics and related reference data.
/// Idempotent: skips if any clinics already exist.
/// </summary>
public static class ClinicalSeeder
{
    private static readonly Action<ILogger, Exception?> LogClinicAlreadySeeded =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(1, nameof(LogClinicAlreadySeeded)),
            "Clinic already seeded.");

    private static readonly Action<ILogger, Exception?> LogExampleClinicSeeded =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(2, nameof(LogExampleClinicSeeded)),
            "Example clinic seeded.");

    private static readonly Action<ILogger, Exception?> LogDemoProviderSeeded =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(3, nameof(LogDemoProviderSeeded)),
            "Demo telehealth provider seeded.");

    private static readonly Action<ILogger, Exception?> LogDemoAssessmentSeeded =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(4, nameof(LogDemoAssessmentSeeded)),
            "Demo mental health assessment seeded.");

    private static readonly Action<ILogger, Exception?> LogDemoInpatientSeeded =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(5, nameof(LogDemoInpatientSeeded)),
            "Demo inpatient capacity seeded.");

    /// <summary>
    /// Seeds a default demo clinic when none exist.
    /// </summary>
    public static async Task SeedAsync(
        IServiceProvider scopedProvider,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var context = scopedProvider.GetService<ClinicalDbContext>();
        if (context == null) return;

        var anyClinics = await context.Clinics.AnyAsync(cancellationToken).ConfigureAwait(false);
        if (!anyClinics)
        {
            var clinic = new Clinic
            {
                Name = "RaphCare Demo Clinic",
                RegistrationNumber = "REG-DEMO-001",
                Country = "South Africa",
                TimeZone = "South Africa Standard Time",
                IsActive = true
            };
            context.Clinics.Add(clinic);
            context.Entry(clinic).Property(c => c.Id).CurrentValue = ClinicalSeedIds.DemoClinicId;
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            LogExampleClinicSeeded(logger, null);
        }
        else
        {
            LogClinicAlreadySeeded(logger, null);
        }

        await EnsureDemoTelehealthProviderAsync(context, logger, cancellationToken).ConfigureAwait(false);
        await EnsureDemoMentalHealthAssessmentAsync(context, logger, cancellationToken).ConfigureAwait(false);
        await EnsureDemoInpatientCapacityAsync(context, logger, cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsureDemoTelehealthProviderAsync(
        ClinicalDbContext context,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var providers = context.Set<Provider>();
        if (await providers.AnyAsync(p => p.IsActive && !p.IsDeleted, cancellationToken).ConfigureAwait(false))
            return;

        var clinicId = await context.Clinics
            .Where(c => c.IsActive)
            .OrderBy(c => c.Id == ClinicalSeedIds.DemoClinicId ? 0 : 1)
            .ThenBy(c => c.CreatedAt)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (clinicId == Guid.Empty)
            return;

        var provider = new Provider
        {
            ClinicId = clinicId,
            ApplicationUserId = ClinicalSeedIds.DemoProviderApplicationUserId,
            LicenseNumber = "DEMO-LIC-001",
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        };
        providers.Add(provider);
        context.Entry(provider).Property(p => p.Id).CurrentValue = ClinicalSeedIds.DemoProviderId;
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        LogDemoProviderSeeded(logger, null);
    }

    private static async Task EnsureDemoMentalHealthAssessmentAsync(
        ClinicalDbContext context,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (await context.MentalHealthAssessments.AnyAsync(cancellationToken).ConfigureAwait(false))
            return;

        var clinicId = await context.Clinics
            .Where(c => c.IsActive)
            .OrderBy(c => c.Id == ClinicalSeedIds.DemoClinicId ? 0 : 1)
            .ThenBy(c => c.CreatedAt)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        if (clinicId == Guid.Empty)
            return;

        var patient = await context.Patients
            .FirstOrDefaultAsync(p => p.Id == ClinicalSeedIds.DemoPatientId && !p.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (patient is null)
        {
            patient = new Patient
            {
                FirstName = "Demo",
                LastName = "Patient",
                DateOfBirth = new DateTime(1990, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                Gender = "Unknown",
                PhoneNumber = "+27000000001",
                Email = "demo.patient@raphcare.local",
                IsActive = true
            };
            context.Patients.Add(patient);
            context.Entry(patient).Property(p => p.Id).CurrentValue = ClinicalSeedIds.DemoPatientId;
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        var hasAccess = await context.PatientClinicAccesses
            .AnyAsync(a => a.PatientId == patient.Id && a.ClinicId == clinicId, cancellationToken)
            .ConfigureAwait(false);
        if (!hasAccess)
        {
            context.PatientClinicAccesses.Add(new PatientClinicAccess
            {
                PatientId = patient.Id,
                ClinicId = clinicId,
                AccessType = PatientClinicAccessType.ManualGrant,
                GrantedAt = DateTime.UtcNow,
                GrantedByRule = "ClinicalSeeder",
                IsActive = true
            });
        }

        var assessment = new MentalHealthAssessment
        {
            ClinicId = clinicId,
            PatientId = patient.Id,
            AssessmentType = "PHQ-9",
            ConductedAt = DateTime.UtcNow.AddDays(-2),
            TotalScore = 8,
            SeverityLevel = "Mild",
            IsAIEnhanced = false
        };
        context.MentalHealthAssessments.Add(assessment);
        context.Entry(assessment).Property(a => a.Id).CurrentValue = ClinicalSeedIds.DemoMentalHealthAssessmentId;
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        LogDemoAssessmentSeeded(logger, null);
    }

    private static async Task EnsureDemoInpatientCapacityAsync(
        ClinicalDbContext context,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (await context.Wards.AnyAsync(cancellationToken).ConfigureAwait(false))
            return;

        var clinicId = await context.Clinics
            .Where(c => c.IsActive)
            .OrderBy(c => c.Id == ClinicalSeedIds.DemoClinicId ? 0 : 1)
            .ThenBy(c => c.CreatedAt)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        if (clinicId == Guid.Empty)
            return;

        var facilities = context.Set<Facility>();
        var facility = await facilities
            .FirstOrDefaultAsync(f => f.Id == ClinicalSeedIds.DemoFacilityId, cancellationToken)
            .ConfigureAwait(false);
        if (facility is null)
        {
            facility = new Facility
            {
                ClinicId = clinicId,
                Name = "Main Campus",
                Address = "1 Demo Street",
                City = "Cape Town",
                Country = "South Africa",
                IsVirtual = false
            };
            facilities.Add(facility);
            context.Entry(facility).Property(f => f.Id).CurrentValue = ClinicalSeedIds.DemoFacilityId;
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        var ward = new Ward
        {
            ClinicId = clinicId,
            FacilityId = facility.Id,
            Name = "Medical Ward A",
            Code = "MED-A",
            IsActive = true
        };
        context.Wards.Add(ward);
        context.Entry(ward).Property(w => w.Id).CurrentValue = ClinicalSeedIds.DemoWardId;

        var room = new Room
        {
            WardId = ClinicalSeedIds.DemoWardId,
            Name = "101",
            RoomType = "Semi-private",
            IsActive = true
        };
        context.Rooms.Add(room);
        context.Entry(room).Property(r => r.Id).CurrentValue = ClinicalSeedIds.DemoRoomId;

        var bedA = new Bed
        {
            RoomId = ClinicalSeedIds.DemoRoomId,
            Label = "A",
            Status = "Available",
            IsActive = true
        };
        context.Beds.Add(bedA);
        context.Entry(bedA).Property(b => b.Id).CurrentValue = ClinicalSeedIds.DemoBedAId;

        var bedB = new Bed
        {
            RoomId = ClinicalSeedIds.DemoRoomId,
            Label = "B",
            Status = "Available",
            IsActive = true
        };
        context.Beds.Add(bedB);
        context.Entry(bedB).Property(b => b.Id).CurrentValue = ClinicalSeedIds.DemoBedBId;

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        LogDemoInpatientSeeded(logger, null);
    }
}
