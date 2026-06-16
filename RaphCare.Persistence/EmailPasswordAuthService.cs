using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Identity;
using RaphCare.Domain.Patients;

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

        _identityDbContext.Users.Add(user);
        _identityDbContext.EmailPasswordCredentials.Add(credential);
        _clinicalDbContext.Patients.Add(patient);
        await _identityDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await _clinicalDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var patientRole = await _identityDbContext.Roles
            .FirstOrDefaultAsync(r => r.Name == "Patient", cancellationToken)
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
            return (false, "Patient profile is missing.", null, Guid.Empty);

        return (true, null, user, patientId);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
