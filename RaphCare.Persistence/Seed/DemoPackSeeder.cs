using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Identity;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Patients;
using RaphCare.Domain.Patients.Enums;

namespace RaphCare.Persistence.Seed;

/// <summary>
/// Staging-only demo clinic, staff and patient accounts, and story data.
/// Idempotent. Does not delete other hospitals.
/// </summary>
public static class DemoPackSeeder
{
    public const string PasswordConfigKey = "Demo:Password";
    public const string DefaultStagingPassword = "RaphCareDemo!2026";

    private static readonly PasswordHasher<string> PasswordHasher = new();

    private static readonly Action<ILogger, Exception?> LogPackReady =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(1, nameof(LogPackReady)),
            "Staging demo pack is ready (RaphCare Demo Clinic + demo.*@raphcare.com accounts).");

    private static readonly Action<ILogger, Exception?> LogPackFailed =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(2, nameof(LogPackFailed)),
            "Staging demo pack failed. The API will still start. Other hospitals were not deleted.");

    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("RaphCare.Persistence.Seed.DemoPackSeeder");

        var identity = serviceProvider.GetService<IdentityDbContext>();
        var clinical = serviceProvider.GetService<ClinicalDbContext>();
        var config = serviceProvider.GetService<IConfiguration>();
        var roles = serviceProvider.GetService<IUserRoleAssignmentService>();
        if (identity is null || clinical is null || roles is null)
            return;

        var password = config?[PasswordConfigKey];
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            password = DefaultStagingPassword;

        try
        {
            await EnsureDemoClinicAsync(clinical, cancellationToken).ConfigureAwait(false);
            await EnsureDemoCapacityAsync(clinical, cancellationToken).ConfigureAwait(false);
            await MigrateLegacyDemoEmailsAsync(identity, cancellationToken).ConfigureAwait(false);
            await EnsureAccountsAsync(identity, roles, password, cancellationToken).ConfigureAwait(false);
            await EnsureDemoProviderAsync(clinical, cancellationToken).ConfigureAwait(false);
            await EnsureMembershipsAsync(clinical, cancellationToken).ConfigureAwait(false);
            await EnsurePatientsAsync(clinical, cancellationToken).ConfigureAwait(false);
            await EnsureStoryDataAsync(clinical, cancellationToken).ConfigureAwait(false);
            LogPackReady(logger, null);
        }
        catch (Exception ex)
        {
            LogPackFailed(logger, ex);
        }
    }

    private static async Task MigrateLegacyDemoEmailsAsync(
        IdentityDbContext identity,
        CancellationToken cancellationToken)
    {
        // Older staging seeds used @raphcare.demo. Rewrite to the documented @raphcare.com addresses.
        string[] pairs =
        [
            DemoPackAccounts.AdminEmail,
            DemoPackAccounts.DoctorEmail,
            DemoPackAccounts.PharmacistEmail,
            DemoPackAccounts.LabEmail,
            DemoPackAccounts.PatientEmail
        ];

        foreach (var canonical in pairs)
        {
            var legacy = DemoPackAccounts.ToLegacyDemoEmail(canonical);
            await identity.Database
                .ExecuteSqlInterpolatedAsync(
                    $"UPDATE ApplicationUsers SET Email = {canonical} WHERE Email = {legacy}",
                    cancellationToken)
                .ConfigureAwait(false);
            await identity.Database
                .ExecuteSqlInterpolatedAsync(
                    $"UPDATE EmailPasswordCredentials SET Email = {canonical} WHERE Email = {legacy}",
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private static async Task EnsureDemoClinicAsync(ClinicalDbContext clinical, CancellationToken cancellationToken)
    {
        var clinic = await clinical.Clinics
            .FirstOrDefaultAsync(c => c.Id == ClinicalSeedIds.DemoClinicId, cancellationToken)
            .ConfigureAwait(false);

        if (clinic is null)
        {
            clinic = new Clinic
            {
                Name = "RaphCare Demo Clinic",
                RegistrationNumber = "REG-DEMO-001",
                ReferenceCode = "RC-DEMCLN",
                Country = "South Africa",
                TimeZone = "South Africa Standard Time",
                IsActive = true,
                RegisteredByApplicationUserId = ClinicalSeedIds.DemoAdminUserId
            };
            clinical.Clinics.Add(clinic);
            clinical.Entry(clinic).Property(c => c.Id).CurrentValue = ClinicalSeedIds.DemoClinicId;
            await clinical.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        if (string.IsNullOrWhiteSpace(clinic.CollectionDisplayToken))
        {
            clinic.CollectionDisplayToken = ClinicCollectionDisplayToken.Generate();
            await clinical.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task EnsureDemoCapacityAsync(ClinicalDbContext clinical, CancellationToken cancellationToken)
    {
        if (await clinical.Wards.AnyAsync(w => w.Id == ClinicalSeedIds.DemoWardId, cancellationToken).ConfigureAwait(false))
            return;

        var facilities = clinical.Set<Facility>();
        var facility = await facilities
            .FirstOrDefaultAsync(f => f.Id == ClinicalSeedIds.DemoFacilityId, cancellationToken)
            .ConfigureAwait(false);
        if (facility is null)
        {
            facility = new Facility
            {
                ClinicId = ClinicalSeedIds.DemoClinicId,
                Name = "Demo Main Campus",
                Address = "1 Demo Street",
                City = "Cape Town",
                Country = "South Africa",
                IsVirtual = false
            };
            facilities.Add(facility);
            clinical.Entry(facility).Property(f => f.Id).CurrentValue = ClinicalSeedIds.DemoFacilityId;
            await clinical.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        var ward = new Ward
        {
            ClinicId = ClinicalSeedIds.DemoClinicId,
            FacilityId = facility.Id,
            Name = "Medical Ward A",
            Code = "MED-A",
            IsActive = true
        };
        clinical.Wards.Add(ward);
        clinical.Entry(ward).Property(w => w.Id).CurrentValue = ClinicalSeedIds.DemoWardId;

        var room = new Room
        {
            WardId = ClinicalSeedIds.DemoWardId,
            Name = "101",
            RoomType = "Semi-private",
            IsActive = true
        };
        clinical.Rooms.Add(room);
        clinical.Entry(room).Property(r => r.Id).CurrentValue = ClinicalSeedIds.DemoRoomId;

        var bedA = new Bed
        {
            RoomId = ClinicalSeedIds.DemoRoomId,
            Label = "A",
            Status = "Available",
            IsActive = true
        };
        clinical.Beds.Add(bedA);
        clinical.Entry(bedA).Property(b => b.Id).CurrentValue = ClinicalSeedIds.DemoBedAId;

        var bedB = new Bed
        {
            RoomId = ClinicalSeedIds.DemoRoomId,
            Label = "B",
            Status = "Available",
            IsActive = true
        };
        clinical.Beds.Add(bedB);
        clinical.Entry(bedB).Property(b => b.Id).CurrentValue = ClinicalSeedIds.DemoBedBId;

        await clinical.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsureAccountsAsync(
        IdentityDbContext identity,
        IUserRoleAssignmentService roles,
        string password,
        CancellationToken cancellationToken)
    {
        await EnsureStaffAsync(
            identity,
            roles,
            ClinicalSeedIds.DemoAdminUserId,
            DemoPackAccounts.AdminEmail,
            "Demo Admin",
            "local-demo-admin",
            password,
            RaphCareRoles.Administrator,
            jobRole: null,
            cancellationToken).ConfigureAwait(false);

        await EnsureStaffAsync(
            identity,
            roles,
            ClinicalSeedIds.DemoProviderApplicationUserId,
            DemoPackAccounts.DoctorEmail,
            "Demo Doctor",
            "local-demo-doctor",
            password,
            RaphCareRoles.Doctor,
            RaphCareRoles.Doctor,
            cancellationToken).ConfigureAwait(false);

        await EnsureStaffAsync(
            identity,
            roles,
            ClinicalSeedIds.DemoPharmacistUserId,
            DemoPackAccounts.PharmacistEmail,
            "Demo Pharmacist",
            "local-demo-pharmacy",
            password,
            RaphCareRoles.Pharmacist,
            RaphCareRoles.Pharmacist,
            cancellationToken).ConfigureAwait(false);

        await EnsureStaffAsync(
            identity,
            roles,
            ClinicalSeedIds.DemoLabUserId,
            DemoPackAccounts.LabEmail,
            "Demo Lab Technician",
            "local-demo-lab",
            password,
            RaphCareRoles.LabTechnician,
            RaphCareRoles.LabTechnician,
            cancellationToken).ConfigureAwait(false);

        await EnsureStaffAsync(
            identity,
            roles,
            ClinicalSeedIds.DemoPatientUserId,
            DemoPackAccounts.PatientEmail,
            "Demo Patient",
            "local-demo-patient",
            password,
            RaphCareRoles.Patient,
            jobRole: null,
            cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsureStaffAsync(
        IdentityDbContext identity,
        IUserRoleAssignmentService roles,
        Guid userId,
        string email,
        string displayName,
        string entraObjectId,
        string password,
        string primaryRole,
        string? jobRole,
        CancellationToken cancellationToken)
    {
        var legacyEmail = DemoPackAccounts.ToLegacyDemoEmail(email);

        var user = await identity.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            user = await identity.Users
                .FirstOrDefaultAsync(u => u.Email == email || u.Email == legacyEmail, cancellationToken)
                .ConfigureAwait(false);
        }

        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = userId,
                EntraObjectId = entraObjectId,
                Email = email,
                DisplayName = displayName,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };
            identity.Users.Add(user);
            identity.Entry(user).Property(u => u.Id).CurrentValue = userId;
            await identity.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            // Rewrite legacy @raphcare.demo addresses to the documented @raphcare.com emails.
            user.Email = email;
            user.DisplayName = displayName;
            user.IsActive = true;
            user.IsDeleted = false;
            identity.Entry(user).Property(u => u.Email).IsModified = true;
            await identity.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        var credential = await identity.EmailPasswordCredentials
            .FirstOrDefaultAsync(
                c => c.UserId == user.Id || c.Email == email || c.Email == legacyEmail,
                cancellationToken)
            .ConfigureAwait(false);
        var hash = PasswordHasher.HashPassword(email, password);
        if (credential is null)
        {
            identity.EmailPasswordCredentials.Add(new EmailPasswordCredential
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Email = email,
                PasswordHash = hash,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            credential.UserId = user.Id;
            credential.Email = email;
            credential.PasswordHash = hash;
            credential.UpdatedAt = DateTime.UtcNow;
            identity.Entry(credential).Property(c => c.Email).IsModified = true;
            identity.Entry(credential).Property(c => c.PasswordHash).IsModified = true;
        }

        await identity.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Drop any leftover legacy-domain credential rows for this account.
        var orphanLegacy = await identity.EmailPasswordCredentials
            .Where(c => c.Email == legacyEmail && c.UserId != user.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        if (orphanLegacy.Count > 0)
        {
            identity.EmailPasswordCredentials.RemoveRange(orphanLegacy);
            await identity.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        await roles.AssignRoleIfMissingAsync(user.Id, primaryRole, cancellationToken).ConfigureAwait(false);
        if (jobRole is not null)
            await roles.SetStaffJobRoleAsync(user.Id, jobRole, cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsureDemoProviderAsync(ClinicalDbContext clinical, CancellationToken cancellationToken)
    {
        var providers = clinical.Set<Provider>();
        var provider = await providers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == ClinicalSeedIds.DemoProviderId, cancellationToken)
            .ConfigureAwait(false);

        if (provider is null)
        {
            provider = await providers
                .FirstOrDefaultAsync(
                    p => p.ClinicId == ClinicalSeedIds.DemoClinicId && p.IsActive && !p.IsDeleted,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        if (provider is null)
        {
            provider = new Provider
            {
                ClinicId = ClinicalSeedIds.DemoClinicId,
                ApplicationUserId = ClinicalSeedIds.DemoProviderApplicationUserId,
                LicenseNumber = "DEMO-LIC-001",
                IsActive = true,
                JoinedAt = DateTime.UtcNow
            };
            providers.Add(provider);
            clinical.Entry(provider).Property(p => p.Id).CurrentValue = ClinicalSeedIds.DemoProviderId;
        }
        else
        {
            provider.ApplicationUserId = ClinicalSeedIds.DemoProviderApplicationUserId;
            provider.IsActive = true;
            provider.IsDeleted = false;
        }

        await clinical.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsureMembershipsAsync(ClinicalDbContext clinical, CancellationToken cancellationToken)
    {
        Guid[] staff =
        [
            ClinicalSeedIds.DemoAdminUserId,
            ClinicalSeedIds.DemoProviderApplicationUserId,
            ClinicalSeedIds.DemoPharmacistUserId,
            ClinicalSeedIds.DemoLabUserId
        ];

        foreach (var userId in staff)
        {
            var exists = await clinical.ClinicStaffMemberships
                .AnyAsync(
                    m => m.ApplicationUserId == userId && m.ClinicId == ClinicalSeedIds.DemoClinicId,
                    cancellationToken)
                .ConfigureAwait(false);
            if (exists)
                continue;

            clinical.ClinicStaffMemberships.Add(new ClinicStaffMembership
            {
                ApplicationUserId = userId,
                ClinicId = ClinicalSeedIds.DemoClinicId,
                IsActive = true,
                JoinedAt = DateTime.UtcNow
            });
        }

        await clinical.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsurePatientsAsync(ClinicalDbContext clinical, CancellationToken cancellationToken)
    {
        var patient = await clinical.Patients
            .FirstOrDefaultAsync(p => p.Id == ClinicalSeedIds.DemoPatientId, cancellationToken)
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
                Email = DemoPackAccounts.PatientEmail,
                IsActive = true
            };
            patient.LinkToApplicationUser(ClinicalSeedIds.DemoPatientUserId);
            clinical.Patients.Add(patient);
            clinical.Entry(patient).Property(p => p.Id).CurrentValue = ClinicalSeedIds.DemoPatientId;
            await clinical.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            if (patient.ApplicationUserId is null
                || patient.ApplicationUserId != ClinicalSeedIds.DemoPatientUserId)
            {
                patient.LinkToApplicationUser(ClinicalSeedIds.DemoPatientUserId);
                clinical.Entry(patient).Property(p => p.ApplicationUserId).IsModified = true;
            }

            patient.Email = DemoPackAccounts.PatientEmail;
            clinical.Entry(patient).Property(p => p.Email).IsModified = true;
            await clinical.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        await GrantAccessAsync(clinical, patient.Id, cancellationToken).ConfigureAwait(false);

        var inpatient = await clinical.Patients
            .FirstOrDefaultAsync(p => p.Id == ClinicalSeedIds.DemoInpatientPatientId, cancellationToken)
            .ConfigureAwait(false);
        if (inpatient is null)
        {
            inpatient = new Patient
            {
                FirstName = "Demo",
                LastName = "Inpatient",
                DateOfBirth = new DateTime(1978, 6, 2, 0, 0, 0, DateTimeKind.Utc),
                Gender = "Unknown",
                PhoneNumber = "+27000000002",
                Email = "demo.inpatient@raphcare.com",
                IsActive = true
            };
            clinical.Patients.Add(inpatient);
            clinical.Entry(inpatient).Property(p => p.Id).CurrentValue = ClinicalSeedIds.DemoInpatientPatientId;
            await clinical.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            inpatient.Email = "demo.inpatient@raphcare.com";
            await clinical.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task GrantAccessAsync(ClinicalDbContext clinical, Guid patientId, CancellationToken cancellationToken)
    {
        var hasAccess = await clinical.PatientClinicAccesses
            .AnyAsync(
                a => a.PatientId == patientId && a.ClinicId == ClinicalSeedIds.DemoClinicId,
                cancellationToken)
            .ConfigureAwait(false);
        if (hasAccess)
            return;

        clinical.PatientClinicAccesses.Add(new PatientClinicAccess
        {
            PatientId = patientId,
            ClinicId = ClinicalSeedIds.DemoClinicId,
            AccessType = PatientClinicAccessType.ManualGrant,
            GrantedAt = DateTime.UtcNow,
            GrantedByRule = "DemoPackSeeder",
            LastValidatedAt = DateTime.UtcNow,
            IsActive = true
        });
        await clinical.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsureStoryDataAsync(ClinicalDbContext clinical, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        if (!await clinical.Appointments.AnyAsync(a => a.Id == ClinicalSeedIds.DemoAppointmentId, cancellationToken).ConfigureAwait(false))
        {
            var appointment = new Appointment
            {
                ClinicId = ClinicalSeedIds.DemoClinicId,
                PatientId = ClinicalSeedIds.DemoPatientId,
                ProviderId = ClinicalSeedIds.DemoProviderId,
                FacilityId = ClinicalSeedIds.DemoFacilityId,
                ScheduledStart = now.AddHours(-3),
                ScheduledEnd = now.AddHours(-2),
                Type = "InPerson",
                Status = "Completed",
                Reason = "Demo follow-up"
            };
            clinical.Appointments.Add(appointment);
            clinical.Entry(appointment).Property(a => a.Id).CurrentValue = ClinicalSeedIds.DemoAppointmentId;
            await clinical.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        if (!await clinical.Visits.AnyAsync(v => v.Id == ClinicalSeedIds.DemoVisitId, cancellationToken).ConfigureAwait(false))
        {
            var visit = new Visit
            {
                ClinicId = ClinicalSeedIds.DemoClinicId,
                AppointmentId = ClinicalSeedIds.DemoAppointmentId,
                PatientId = ClinicalSeedIds.DemoPatientId,
                ProviderId = ClinicalSeedIds.DemoProviderId,
                VisitStart = now.AddHours(-3),
                VisitEnd = now.AddHours(-2),
                VisitType = "InPerson",
                Status = "Completed",
                Summary = "Demo visit for collection and records."
            };
            clinical.Visits.Add(visit);
            clinical.Entry(visit).Property(v => v.Id).CurrentValue = ClinicalSeedIds.DemoVisitId;
            await clinical.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        if (!await clinical.Set<Prescription>().AnyAsync(p => p.Id == ClinicalSeedIds.DemoPrescriptionId, cancellationToken).ConfigureAwait(false))
        {
            var rx = new Prescription
            {
                VisitId = ClinicalSeedIds.DemoVisitId,
                IssuedAt = now.AddHours(-2),
                Status = "Pending",
                PickupCode = "2DEM2A",
                Notes = "Demo prescription"
            };
            clinical.Set<Prescription>().Add(rx);
            clinical.Entry(rx).Property(p => p.Id).CurrentValue = ClinicalSeedIds.DemoPrescriptionId;
            clinical.Set<PrescriptionItem>().Add(new PrescriptionItem
            {
                PrescriptionId = ClinicalSeedIds.DemoPrescriptionId,
                MedicationName = "Amoxicillin",
                Dosage = "500 mg",
                Frequency = "Three times a day",
                DurationDays = 5
            });
            await clinical.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        if (!await clinical.Set<LabRequest>().AnyAsync(l => l.Id == ClinicalSeedIds.DemoLabRequestId, cancellationToken).ConfigureAwait(false))
        {
            var lab = new LabRequest
            {
                VisitId = ClinicalSeedIds.DemoVisitId,
                TestName = "Full blood count",
                Priority = "Routine",
                RequestedAt = now.AddHours(-2),
                Status = "Pending",
                PickupCode = "2DEM2B"
            };
            clinical.Set<LabRequest>().Add(lab);
            clinical.Entry(lab).Property(l => l.Id).CurrentValue = ClinicalSeedIds.DemoLabRequestId;
            await clinical.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        if (!await clinical.Set<Prescription>().AnyAsync(p => p.Id == ClinicalSeedIds.DemoCalledPrescriptionId, cancellationToken).ConfigureAwait(false))
        {
            var called = new Prescription
            {
                VisitId = ClinicalSeedIds.DemoVisitId,
                IssuedAt = now.AddHours(-1),
                Status = "Pending",
                PickupCode = "2DEM2C",
                CalledAt = now.AddMinutes(-10),
                Notes = "Demo called to counter"
            };
            clinical.Set<Prescription>().Add(called);
            clinical.Entry(called).Property(p => p.Id).CurrentValue = ClinicalSeedIds.DemoCalledPrescriptionId;
            clinical.Set<PrescriptionItem>().Add(new PrescriptionItem
            {
                PrescriptionId = ClinicalSeedIds.DemoCalledPrescriptionId,
                MedicationName = "Paracetamol",
                Dosage = "500 mg",
                Frequency = "As needed",
                DurationDays = 3
            });
            await clinical.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        if (!await clinical.InpatientAdmissions.AnyAsync(a => a.Id == ClinicalSeedIds.DemoAdmissionId, cancellationToken).ConfigureAwait(false))
        {
            var bed = await clinical.Beds
                .FirstOrDefaultAsync(b => b.Id == ClinicalSeedIds.DemoBedBId, cancellationToken)
                .ConfigureAwait(false);
            var admission = new InpatientAdmission
            {
                ClinicId = ClinicalSeedIds.DemoClinicId,
                PatientId = ClinicalSeedIds.DemoInpatientPatientId,
                BedId = ClinicalSeedIds.DemoBedBId,
                AdmittedAt = now.AddHours(-6),
                Status = "Admitted",
                Reason = "Demo overnight stay",
                AdmittedByApplicationUserId = ClinicalSeedIds.DemoAdminUserId
            };
            clinical.InpatientAdmissions.Add(admission);
            clinical.Entry(admission).Property(a => a.Id).CurrentValue = ClinicalSeedIds.DemoAdmissionId;
            if (bed is not null)
                bed.Status = "Occupied";
            await clinical.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
