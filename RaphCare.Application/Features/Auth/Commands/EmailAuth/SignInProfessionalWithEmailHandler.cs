using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Identity;

namespace RaphCare.Application.Features.Auth.Commands.EmailAuth;

public sealed class SignInProfessionalWithEmailHandler(
    IEmailPasswordAuthService emailPasswordAuthService,
    IEmailOtpService emailOtpService,
    IEmailService emailService,
    IUserRoleAssignmentService roleAssignmentService,
    ITokenService tokenService,
    IClinicStaffPendingInvitationService clinicStaffPendingInvitationService,
    IIdentityOtpProvisioningService identityOtpProvisioningService,
    IDateTimeProvider dateTimeProvider) : IRequestHandler<SignInProfessionalWithEmailCommand, EmailAuthResult>
{
    public async Task<EmailAuthResult> Handle(SignInProfessionalWithEmailCommand request, CancellationToken cancellationToken)
    {
        var (success, error, user) = await emailPasswordAuthService
            .SignInProfessionalAsync(request.Email, request.Password, cancellationToken)
            .ConfigureAwait(false);

        if (!success || user is null)
            return new EmailAuthResult { Success = false, Error = error ?? "Sign-in failed." };

        if (string.IsNullOrWhiteSpace(request.VerificationCode))
        {
            if (!DemoPackAccounts.IsDemoEmail(request.Email))
            {
                await EmailVerificationHelper.SendVerificationEmailAsync(
                    request.Email, emailOtpService, emailService, cancellationToken).ConfigureAwait(false);
                return new EmailAuthResult { Success = true, RequiresVerification = true };
            }
        }
        else if (!await EmailVerificationHelper.ValidateCodeAsync(request.Email, request.VerificationCode, emailOtpService, cancellationToken)
                     .ConfigureAwait(false))
        {
            return new EmailAuthResult { Success = false, Error = "Invalid or expired verification code." };
        }

        await clinicStaffPendingInvitationService
            .AcceptPendingInvitationsAsync(user.Id, request.Email, cancellationToken)
            .ConfigureAwait(false);

        var assigned = await roleAssignmentService.GetRoleNamesAsync(user.Id, cancellationToken).ConfigureAwait(false);
        if (!RaphCareRoles.HasProviderJobRole(assigned))
            await roleAssignmentService.AssignRoleIfMissingAsync(user.Id, RaphCareRoles.Clinician, cancellationToken).ConfigureAwait(false);

        var roles = await roleAssignmentService.GetRoleNamesAsync(user.Id, cancellationToken).ConfigureAwait(false);

        await identityOtpProvisioningService.LogLoginAttemptAsync(new LoginAudit
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            LoginTime = dateTimeProvider.UtcNow,
            Success = true,
            CreatedAt = dateTimeProvider.UtcNow
        }, cancellationToken).ConfigureAwait(false);

        var token = tokenService.GenerateStaffToken(user, roles);
        return new EmailAuthResult { Success = true, Token = token };
    }
}
