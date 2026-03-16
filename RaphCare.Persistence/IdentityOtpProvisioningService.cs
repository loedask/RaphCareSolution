using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Identity;
using RaphCare.Domain.Patients;

namespace RaphCare.Persistence;

/// <summary>
/// EF Core implementation of OTP-based identity provisioning and patient linking,
/// using <see cref="IdentityDbContext"/> and <see cref="ClinicalDbContext"/>.
/// </summary>
public class IdentityOtpProvisioningService : IIdentityOtpProvisioningService
{
    private readonly IdentityDbContext _identityDbContext;
    private readonly ClinicalDbContext _clinicalDbContext;
    private readonly IDateTimeProvider _clock;

    public IdentityOtpProvisioningService(
        IdentityDbContext identityDbContext,
        ClinicalDbContext clinicalDbContext,
        IDateTimeProvider clock)
    {
        _identityDbContext = identityDbContext ?? throw new ArgumentNullException(nameof(identityDbContext));
        _clinicalDbContext = clinicalDbContext ?? throw new ArgumentNullException(nameof(clinicalDbContext));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

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
        var patient = await _clinicalDbContext.Patients
            .FirstOrDefaultAsync(p => p.ApplicationUserId == user.Id, cancellationToken);

        if (patient is null)
        {
            // Try to backfill an existing patient created via other flows using the same phone number.
            patient = await _clinicalDbContext.Patients
                .FirstOrDefaultAsync(p => p.PhoneNumber == normalizedPhone, cancellationToken);

            if (patient is null)
            {
                patient = new Patient
                {
                    ClinicId = Guid.Empty,
                    FirstName = normalizedPhone,
                    LastName = string.Empty,
                    DateOfBirth = DateTime.UtcNow, // placeholder until onboarding collects real DOB
                    PhoneNumber = normalizedPhone,
                    IsActive = true
                };

                await _clinicalDbContext.Patients.AddAsync(patient, cancellationToken);
            }

            patient.LinkToApplicationUser(user.Id);
            await _clinicalDbContext.SaveChangesAsync(cancellationToken);
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

