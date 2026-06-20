using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Identity;

namespace RaphCare.Application.Features.Auth.Commands.EmailAuth;

public sealed class RegisterProfessionalWithEmailHandler(
    IEmailPasswordAuthService emailPasswordAuthService,
    IEmailOtpService emailOtpService,
    IUserRoleAssignmentService roleAssignmentService,
    ITokenService tokenService,
    IClinicStaffPendingInvitationService clinicStaffPendingInvitationService,
    IIdentityOtpProvisioningService identityOtpProvisioningService,
    IDateTimeProvider dateTimeProvider) : IRequestHandler<RegisterProfessionalWithEmailCommand, EmailAuthResult>
{
    public async Task<EmailAuthResult> Handle(RegisterProfessionalWithEmailCommand request, CancellationToken cancellationToken)
    {
        if (!await EmailVerificationHelper.ValidateCodeAsync(request.Email, request.VerificationCode, emailOtpService, cancellationToken)
                .ConfigureAwait(false))
        {
            return new EmailAuthResult { Success = false, Error = "Invalid or expired verification code." };
        }

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

        await roleAssignmentService.AssignRoleIfMissingAsync(user.Id, "Clinician", cancellationToken).ConfigureAwait(false);
        await clinicStaffPendingInvitationService
            .AcceptPendingInvitationsAsync(user.Id, request.Email, cancellationToken)
            .ConfigureAwait(false);

        await identityOtpProvisioningService.LogLoginAttemptAsync(new LoginAudit
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            LoginTime = dateTimeProvider.UtcNow,
            Success = true,
            CreatedAt = dateTimeProvider.UtcNow
        }, cancellationToken).ConfigureAwait(false);

        var roles = await roleAssignmentService.GetRoleNamesAsync(user.Id, cancellationToken).ConfigureAwait(false);
        var token = tokenService.GenerateStaffToken(user, roles);
        return new EmailAuthResult { Success = true, Token = token };
    }
}
