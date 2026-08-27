using System.Text;
using RaphCare.Application.Common.Email;
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
        var content = VerificationEmail.Create(code);
        await emailService
            .SendEmailAsync(email.Trim(), content.Subject, content.PlainBody, content.HtmlBody, cancellationToken)
            .ConfigureAwait(false);
    }

    internal static string NormalizeCode(string? code)
    {
        if (string.IsNullOrEmpty(code))
            return string.Empty;

        var digits = new StringBuilder(code.Length);
        foreach (var c in code)
        {
            if (char.IsAsciiDigit(c))
                digits.Append(c);
        }

        return digits.ToString();
    }

    internal static async Task<bool> ValidateCodeAsync(
        string email,
        string? code,
        IEmailOtpService emailOtpService,
        CancellationToken cancellationToken)
    {
        var normalized = NormalizeCode(code);
        if (normalized.Length != 6)
            return false;

        return await emailOtpService.ValidateOtpAsync(email, normalized, cancellationToken).ConfigureAwait(false);
    }
}
