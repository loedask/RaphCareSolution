namespace RaphCare.Application.Common.Interfaces;

/// <summary>Generates and validates short-lived email verification codes (stored hashed in Identity DB).</summary>
public interface IEmailOtpService
{
    Task<string> GenerateOtpAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ValidateOtpAsync(string email, string code, CancellationToken cancellationToken = default);
}
