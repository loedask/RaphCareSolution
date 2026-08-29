using MediatR;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Auth.Commands.EmailAuth;

public sealed class ConfirmPasswordResetHandler(
    IEmailOtpService emailOtpService,
    IEmailPasswordAuthService emailPasswordAuth) : IRequestHandler<ConfirmPasswordResetCommand, EmailAuthResult>
{
    public async Task<EmailAuthResult> Handle(ConfirmPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var codeOk = await EmailVerificationHelper
            .ValidateCodeAsync(email, request.VerificationCode, emailOtpService, cancellationToken)
            .ConfigureAwait(false);

        if (!codeOk)
            return new EmailAuthResult { Success = false, Error = "That code is invalid or expired." };

        var (success, error) = await emailPasswordAuth
            .ResetPasswordByEmailAsync(email, request.NewPassword, cancellationToken)
            .ConfigureAwait(false);

        if (!success)
            return new EmailAuthResult { Success = false, Error = error ?? "Could not reset password." };

        return new EmailAuthResult { Success = true };
    }
}
