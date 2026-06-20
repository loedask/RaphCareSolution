using MediatR;
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
        var subject = "Your RaphCare verification code";
        var body = $"Your RaphCare verification code is: {code}\n\nThis code expires in 10 minutes.";
        await emailService.SendEmailAsync(email.Trim(), subject, body, cancellationToken).ConfigureAwait(false);
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
