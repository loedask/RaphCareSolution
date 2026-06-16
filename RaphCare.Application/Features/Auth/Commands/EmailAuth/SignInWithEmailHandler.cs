using MediatR;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Auth.Commands.EmailAuth;

public sealed class SignInWithEmailHandler(
    IEmailPasswordAuthService emailPasswordAuthService,
    ITokenService tokenService) : IRequestHandler<SignInWithEmailCommand, EmailAuthResult>
{
    public async Task<EmailAuthResult> Handle(SignInWithEmailCommand request, CancellationToken cancellationToken)
    {
        var (success, error, user, patientId) = await emailPasswordAuthService
            .SignInAsync(request.Email, request.Password, cancellationToken)
            .ConfigureAwait(false);

        if (!success || user is null)
            return new EmailAuthResult { Success = false, Error = error ?? "Sign-in failed." };

        var token = tokenService.GeneratePatientToken(user, patientId);
        return new EmailAuthResult { Success = true, Token = token };
    }
}
