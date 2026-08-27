using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Auth.Commands.EmailAuth;

internal static class EmailVerificationHelper
{
    internal static async Task SendVerificationEmailAsync(
        string email,
        IEmailOtpService emailOtpService,
        IEmailService emailService,
        CancellationToken cancellationToken)
    {
        var code = await emailOtpService.GenerateOtpAsync(email, cancellationToken).ConfigureAwait(false);
        var (plainBody, htmlBody) = VerificationEmail.Create(code);
        await emailService
            .SendEmailAsync(email.Trim(), VerificationEmail.Subject, plainBody, htmlBody, cancellationToken)
            .ConfigureAwait(false);
    }

    internal static async Task<bool> ValidateCodeAsync(
        string email,
        string? code,
        IEmailOtpService emailOtpService,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        return await emailOtpService.ValidateOtpAsync(email, code.Trim(), cancellationToken).ConfigureAwait(false);
    }
}
