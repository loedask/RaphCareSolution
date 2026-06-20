using MediatR;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Auth.Commands.EmailAuth;

public sealed class RegisterWithEmailHandler(
    IEmailPasswordAuthService emailPasswordAuthService,
    IEmailOtpService emailOtpService,
    ITokenService tokenService) : IRequestHandler<RegisterWithEmailCommand, EmailAuthResult>
{
    public async Task<EmailAuthResult> Handle(RegisterWithEmailCommand request, CancellationToken cancellationToken)
    {
        if (!await EmailVerificationHelper.ValidateCodeAsync(request.Email, request.VerificationCode, emailOtpService, cancellationToken)
                .ConfigureAwait(false))
        {
            return new EmailAuthResult { Success = false, Error = "Invalid or expired verification code." };
        }

        var (success, error, user, patientId) = await emailPasswordAuthService
            .RegisterAsync(
                request.FirstName,
                request.LastName,
                request.Email,
                request.Password,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false);

        if (!success || user is null)
            return new EmailAuthResult { Success = false, Error = error ?? "Registration failed." };

        var token = tokenService.GeneratePatientToken(user, patientId);
        return new EmailAuthResult { Success = true, Token = token };
    }
}
