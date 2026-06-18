using MediatR;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Auth.Commands.EmailAuth;

public sealed class RegisterProfessionalWithEmailHandler(
    IEmailPasswordAuthService emailPasswordAuthService,
    IUserRoleAssignmentService roleAssignmentService,
    ITokenService tokenService) : IRequestHandler<RegisterProfessionalWithEmailCommand, EmailAuthResult>
{
    public async Task<EmailAuthResult> Handle(RegisterProfessionalWithEmailCommand request, CancellationToken cancellationToken)
    {
        var (success, error, user) = await emailPasswordAuthService
            .RegisterProfessionalAsync(
                request.FirstName,
                request.LastName,
                request.Email,
                request.Password,
                cancellationToken)
            .ConfigureAwait(false);

        if (!success || user is null)
            return new EmailAuthResult { Success = false, Error = error ?? "Registration failed." };

        var roles = await roleAssignmentService.GetRoleNamesAsync(user.Id, cancellationToken).ConfigureAwait(false);
        var token = tokenService.GenerateStaffToken(user, roles);
        return new EmailAuthResult { Success = true, Token = token };
    }
}
