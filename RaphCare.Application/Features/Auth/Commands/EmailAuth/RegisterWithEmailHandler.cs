using MediatR;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Auth.Commands.EmailAuth;

public sealed class RegisterWithEmailHandler(
    IEmailPasswordAuthService emailPasswordAuthService,
    ITokenService tokenService,
    IClinicContext clinicContext) : IRequestHandler<RegisterWithEmailCommand, EmailAuthResult>
{
    public async Task<EmailAuthResult> Handle(RegisterWithEmailCommand request, CancellationToken cancellationToken)
    {
        if (clinicContext.ClinicId is not { } clinicId || clinicId == Guid.Empty)
            return new EmailAuthResult { Success = false, Error = "X-Clinic-Id header is required." };

        var (success, error, user, patientId) = await emailPasswordAuthService
            .RegisterAsync(request.FirstName, request.LastName, request.Email, request.Password, clinicId, cancellationToken)
            .ConfigureAwait(false);

        if (!success || user is null)
            return new EmailAuthResult { Success = false, Error = error ?? "Registration failed." };

        var token = tokenService.GeneratePatientToken(user, patientId);
        return new EmailAuthResult { Success = true, Token = token };
    }
}
