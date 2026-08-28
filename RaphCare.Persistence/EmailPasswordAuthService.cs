using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Identity;
using RaphCare.Domain.Patients;
using RaphCare.Domain.Patients.Enums;

namespace RaphCare.Persistence;

public class EmailPasswordAuthService(
    IdentityDbContext identityDbContext,
    ClinicalDbContext clinicalDbContext,
    IDateTimeProvider clock) : IEmailPasswordAuthService
{
    private readonly IdentityDbContext _identityDbContext = identityDbContext;
    private readonly ClinicalDbContext _clinicalDbContext = clinicalDbContext;
    private readonly IDateTimeProvider _clock = clock;
    private readonly PasswordHasher<string> _passwordHasher = new();

    public async Task<(bool Success, string? Error, ApplicationUser? User, Guid PatientId)> RegisterAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        Guid? clinicId,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        if (string.IsNullOrWhiteSpace(normalizedEmail))
            return (false, "Email is required.", null, Guid.Empty);
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return (false, "Password must be at least 8 characters.", null, Guid.Empty);

        var exists = await _identityDbContext.EmailPasswordCredentials
            .AnyAsync(x => x.Email == normalizedEmail, cancellationToken)
            .ConfigureAwait(false);
        if (exists)
            return (false, "An account with this email already exists.", null, Guid.Empty);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            EntraObjectId = $"local-{Guid.NewGuid():N}",
            Email = normalizedEmail,
            DisplayName = $"{firstName} {lastName}".Trim(),
            IsActive = true,
            IsDeleted = false,
            CreatedAt = _clock.UtcNow
        };
        if (string.IsNullOrWhiteSpace(user.DisplayName))
            user.DisplayName = normalizedEmail;

        var credential = new EmailPasswordCredential
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.HashPassword(normalizedEmail, password),
            CreatedAt = _clock.UtcNow
        };

        var patient = new Patient
        {
            FirstName = string.IsNullOrWhiteSpace(firstName) ? "Patient" : firstName.Trim(),
            LastName = lastName?.Trim() ?? string.Empty,
            DateOfBirth = DateTime.UtcNow,
            PhoneNumber = string.Empty,
            IsActive = true
        };
        patient.LinkToApplicationUser(user.Id);

        if (clinicId is { } selectedClinicId && selectedClinicId != Guid.Empty)
        {
            var clinicExists = await _clinicalDbContext.Clinics
                .AnyAsync(c => c.Id == selectedClinicId && c.IsActive && !c.IsDeleted, cancellationToken)
                .ConfigureAwait(false);
            if (!clinicExists)
                return (false, "Selected clinic is not available.", null, Guid.Empty);
        }

        _identityDbContext.Users.Add(user);
        _identityDbContext.EmailPasswordCredentials.Add(credential);
        _clinicalDbContext.Patients.Add(patient);
        if (clinicId is { } linkClinicId && linkClinicId != Guid.Empty)
        {
            _clinicalDbContext.PatientClinicAccesses.Add(new PatientClinicAccess
            {
                PatientId = patient.Id,
                ClinicId = linkClinicId,
                AccessType = PatientClinicAccessType.Registered,
                GrantedAt = _clock.UtcNow,
                GrantedByRule = "EmailRegistration",
                LastValidatedAt = _clock.UtcNow,
                IsActive = true
            });
        }

        await _identityDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await _clinicalDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var patientRole = await _identityDbContext.Roles
            .FirstOrDefaultAsync(r => r.Name == RaphCareRoles.Patient, cancellationToken)
            .ConfigureAwait(false);
        if (patientRole is not null)
        {
            _identityDbContext.UserRoles.Add(new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RoleId = patientRole.Id,
                CreatedAt = _clock.UtcNow
            });
            await _identityDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return (true, null, user, patient.Id);
    }

    public async Task<(bool Success, string? Error, ApplicationUser? User, Guid PatientId)> SignInAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        var credential = await _identityDbContext.EmailPasswordCredentials
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken)
            .ConfigureAwait(false);
        if (credential is null)
            return (false, "Invalid email or password.", null, Guid.Empty);

        var verification = _passwordHasher.VerifyHashedPassword(normalizedEmail, credential.PasswordHash, password);
        if (verification == PasswordVerificationResult.Failed)
            return (false, "Invalid email or password.", null, Guid.Empty);

        var user = await _identityDbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == credential.UserId, cancellationToken)
            .ConfigureAwait(false);
        if (user is null || !user.IsActive || user.IsDeleted)
            return (false, "Account is not available.", null, Guid.Empty);

        var patientId = await _clinicalDbContext.Patients
            .Where(p => p.ApplicationUserId == user.Id)
            .Select(p => p.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        if (patientId == Guid.Empty)
        {
            var hasStaffRole = await _identityDbContext.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Join(_identityDbContext.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r.Name)
                .AnyAsync(name => name == RaphCareRoles.Clinician || name == RaphCareRoles.Administrator, cancellationToken)
                .ConfigureAwait(false);
            if (hasStaffRole)
                return (false, "This email is registered as a healthcare professional. Use professional sign-in and the clinic portal—not patient sign-in.", null, Guid.Empty);

            return (false, "Patient profile is missing. Complete patient registration or contact support.", null, Guid.Empty);
        }

        return (true, null, user, patientId);
    }

    public async Task<(bool Success, string? Error, ApplicationUser? User)> RegisterProfessionalAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        if (string.IsNullOrWhiteSpace(normalizedEmail))
            return (false, "Email is required.", null);
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return (false, "Password must be at least 8 characters.", null);

        var exists = await _identityDbContext.EmailPasswordCredentials
            .AnyAsync(x => x.Email == normalizedEmail, cancellationToken)
            .ConfigureAwait(false);
        if (exists)
            return (false, "An account with this email already exists.", null);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            EntraObjectId = $"local-{Guid.NewGuid():N}",
            Email = normalizedEmail,
            DisplayName = $"{firstName} {lastName}".Trim(),
            IsActive = true,
            IsDeleted = false,
            CreatedAt = _clock.UtcNow
        };
        if (string.IsNullOrWhiteSpace(user.DisplayName))
            user.DisplayName = normalizedEmail;

        var credential = new EmailPasswordCredential
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.HashPassword(normalizedEmail, password),
            CreatedAt = _clock.UtcNow
        };

        _identityDbContext.Users.Add(user);
        _identityDbContext.EmailPasswordCredentials.Add(credential);
        await _identityDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var clinicianRole = await _identityDbContext.Roles
            .FirstOrDefaultAsync(r => r.Name == RaphCareRoles.Clinician, cancellationToken)
            .ConfigureAwait(false);
        if (clinicianRole is not null)
        {
            _identityDbContext.UserRoles.Add(new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RoleId = clinicianRole.Id,
                CreatedAt = _clock.UtcNow
            });
            await _identityDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return (true, null, user);
    }

    public async Task<(bool Success, string? Error, ApplicationUser? User)> SignInProfessionalAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        var credential = await _identityDbContext.EmailPasswordCredentials
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken)
            .ConfigureAwait(false);
        if (credential is null)
            return (false, "Invalid email or password.", null);

        var verification = _passwordHasher.VerifyHashedPassword(normalizedEmail, credential.PasswordHash, password);
        if (verification == PasswordVerificationResult.Failed)
            return (false, "Invalid email or password.", null);

        var user = await _identityDbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == credential.UserId, cancellationToken)
            .ConfigureAwait(false);
        if (user is null || !user.IsActive || user.IsDeleted)
            return (false, "Account is not available.", null);

        var hasStaffRole = await _identityDbContext.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Join(_identityDbContext.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r.Name)
            .AnyAsync(name => name == RaphCareRoles.Clinician || name == RaphCareRoles.Administrator, cancellationToken)
            .ConfigureAwait(false);
        if (!hasStaffRole)
            return (false, "This account is not registered as a healthcare professional.", null);

        return (true, null, user);
    }

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            return (false, "User is required.");

        var credential = await _identityDbContext.EmailPasswordCredentials
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken)
            .ConfigureAwait(false);
        if (credential is null)
            return (false, "This account uses Microsoft sign-in. Use the Microsoft password reset option.");

        var verification = _passwordHasher.VerifyHashedPassword(credential.Email, credential.PasswordHash, currentPassword);
        if (verification == PasswordVerificationResult.Failed)
            return (false, "Current password is incorrect.");

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            return (false, "New password must be at least 8 characters.");

        credential.PasswordHash = _passwordHasher.HashPassword(credential.Email, newPassword);
        credential.UpdatedAt = _clock.UtcNow;
        await _identityDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return (true, null);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
