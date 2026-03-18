using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Identity;
using RaphCare.Domain.Patients;
using RaphCare.Domain.Patients.Enums;

namespace RaphCare.Persistence;

/// <summary>
/// EF Core implementation of OTP-based identity provisioning and patient linking,
/// using <see cref="IdentityDbContext"/>, <see cref="ClinicalDbContext"/>, and
/// <see cref="IMasterPatientIndexService"/> to avoid duplicate patients.
/// </summary>
public class IdentityOtpProvisioningService(
    IdentityDbContext identityDbContext,
    ClinicalDbContext clinicalDbContext,
    IMasterPatientIndexService mpi,
    IDateTimeProvider clock,
    IPatientIdentityTimelineService patientIdentityTimelineService) : IIdentityOtpProvisioningService
{
    private readonly IdentityDbContext _identityDbContext = identityDbContext ?? throw new ArgumentNullException(nameof(identityDbContext));
    private readonly ClinicalDbContext _clinicalDbContext = clinicalDbContext ?? throw new ArgumentNullException(nameof(clinicalDbContext));
    private readonly IMasterPatientIndexService _mpi = mpi ?? throw new ArgumentNullException(nameof(mpi));
    private readonly IDateTimeProvider _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    private readonly IPatientIdentityTimelineService _patientIdentityTimelineService = patientIdentityTimelineService;

    public async Task<(ApplicationUser user, Patient patient)> EnsureUserAndPatientForPhoneAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default)
    {
        var normalizedPhone = phoneNumber.Trim();

        // Find or create ApplicationUser for this phone-based patient.
        // We use Email to store the phone identifier in this first iteration.
        var user = await _identityDbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(
                u => u.Email == normalizedPhone,
                cancellationToken);

        if (user == null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                EntraObjectId = string.Empty,
                Email = normalizedPhone,
                DisplayName = normalizedPhone,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = _clock.UtcNow
            };

            _identityDbContext.Users.Add(user);
            await _identityDbContext.SaveChangesAsync(cancellationToken);
        }

        // Ensure Patient entity exists and is linked to this ApplicationUser.
        // Use MPI to resolve existing patient (e.g. from REST or voice onboarding) before creating a new one.
        var patient = await _clinicalDbContext.Patients
            .FirstOrDefaultAsync(p => p.ApplicationUserId == user.Id, cancellationToken);

        if (patient is null)
        {
            var existing = await _mpi.FindMatchAsync(
                nationalHealthId: null,
                sourceSystem: null,
                externalId: null,
                firstName: normalizedPhone,
                lastName: "",
                dateOfBirth: DateTime.UtcNow.Date,
                phoneNumber: normalizedPhone,
                cancellationToken);

            if (existing != null)
            {
                // Re-fetch with tracking so SaveChanges persists the link (DbContext uses NoTracking).
                patient = await _clinicalDbContext.Patients.FindAsync([existing.Id], cancellationToken);
                if (patient != null)
                {
                    var oldApplicationUserId = patient.ApplicationUserId;
                    patient.LinkToApplicationUser(user.Id);
                    await _patientIdentityTimelineService.RecordEventAsync(
                        patient.Id,
                        PatientIdentityEventType.IdentityLinked,
                        new
                        {
                            ApplicationUserId = user.Id,
                            OldApplicationUserId = oldApplicationUserId
                        },
                        performedByUserId: null,
                        cancellationToken);
                    await _clinicalDbContext.SaveChangesAsync(cancellationToken);
                }
            }

            if (patient is null)
            {
                patient = new Patient
                {
                    FirstName = normalizedPhone,
                    LastName = string.Empty,
                    DateOfBirth = DateTime.UtcNow, // placeholder until onboarding collects real DOB
                    PhoneNumber = normalizedPhone,
                    IsActive = true
                };

                _clinicalDbContext.Patients.Add(patient);
                patient.LinkToApplicationUser(user.Id);

                await _patientIdentityTimelineService.RecordEventAsync(
                    patient.Id,
                    PatientIdentityEventType.PatientCreated,
                    new
                    {
                        patient.FirstName,
                        patient.LastName,
                        patient.DateOfBirth,
                        patient.PhoneNumber,
                        patient.NationalHealthId
                    },
                    performedByUserId: null,
                    cancellationToken);

                await _patientIdentityTimelineService.RecordEventAsync(
                    patient.Id,
                    PatientIdentityEventType.IdentityLinked,
                    new
                    {
                        ApplicationUserId = user.Id
                    },
                    performedByUserId: null,
                    cancellationToken);

                await _clinicalDbContext.SaveChangesAsync(cancellationToken);
            }
        }

        // Ensure Patient role is assigned.
        var patientRole = await _identityDbContext.Roles
            .FirstOrDefaultAsync(r => r.Name == "Patient", cancellationToken);

        if (patientRole != null)
        {
            var hasPatientRole = await _identityDbContext.UserRoles
                .AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == patientRole.Id, cancellationToken);

            if (!hasPatientRole)
            {
                var link = new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    RoleId = patientRole.Id,
                    CreatedAt = _clock.UtcNow
                };

                _identityDbContext.UserRoles.Add(link);
                await _identityDbContext.SaveChangesAsync(cancellationToken);
            }
        }

        return (user, patient);
    }

    public async Task LogLoginAttemptAsync(LoginAudit audit, CancellationToken cancellationToken = default)
    {
        _identityDbContext.Add(audit);
        await _identityDbContext.SaveChangesAsync(cancellationToken);
    }
}

