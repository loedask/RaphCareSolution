using RaphCare.Client.Contracts;

namespace RaphCare.Client.Contracts.Interfaces;

public sealed class RegistrationClinicItem
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}

public interface IEmailAuthService
{
    Task<Response<IReadOnlyList<RegistrationClinicItem>>> GetRegistrationClinicsAsync(
        CancellationToken cancellationToken = default);

    Task<Response<EmailAuthResult>> RegisterAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        Guid? clinicId = null,
        string? verificationCode = null,
        CancellationToken cancellationToken = default);

    Task<Response<EmailAuthResult>> SignInAsync(
        string email,
        string password,
        string? verificationCode = null,
        CancellationToken cancellationToken = default);

    Task<Response<EmailAuthResult>> RegisterProfessionalAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        string? verificationCode = null,
        CancellationToken cancellationToken = default);

    Task<Response<EmailAuthResult>> SignInProfessionalAsync(
        string email,
        string password,
        string? verificationCode = null,
        CancellationToken cancellationToken = default);

    Task<Response<object>> SendEmailVerificationAsync(string email, CancellationToken cancellationToken = default);

    Task<Response<object>> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);

    Task<Response<EmailAuthResult>> ConfirmPasswordResetAsync(
        string email,
        string verificationCode,
        string newPassword,
        CancellationToken cancellationToken = default);
}

public sealed class EmailAuthResult
{
    public bool Success { get; init; }
    public string? Token { get; init; }
    public bool RequiresVerification { get; init; }
}
