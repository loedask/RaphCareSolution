using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Identity;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Auth.Commands.VerifyOtp;

public class VerifyOtpHandler(
    IOtpService otpService,
    IIdentityOtpProvisioningService identityOtpProvisioningService,
    IDateTimeProvider clock,
    ICurrentUserService currentUser,
    ITokenService tokenService) : IRequestHandler<VerifyOtpCommand, VerifyOtpResult>
{
    private readonly IOtpService _otpService = otpService;
    private readonly IIdentityOtpProvisioningService _identityOtpProvisioningService = identityOtpProvisioningService;
    private readonly IDateTimeProvider _clock = clock;
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly ITokenService _tokenService = tokenService;

    public async Task<VerifyOtpResult> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        var success = await _otpService.ValidateOtpAsync(request.PhoneNumber, request.Code, cancellationToken);

        // LoginAudit is tied to ApplicationUser; for phone onboarding there may be no user yet.
        // We log with a null user by convention (UserId = Guid.Empty) when OTP fails.
        Guid userIdForAudit = Guid.Empty;

        if (!success)
        {
            var failedAudit = new LoginAudit
            {
                Id = Guid.NewGuid(),
                UserId = userIdForAudit,
                LoginTime = _clock.UtcNow,
                IpAddress = _currentUser.IpAddress,
                UserAgent = _currentUser.UserAgent,
                Success = false,
                CreatedAt = _clock.UtcNow
            };

            await _identityOtpProvisioningService.LogLoginAttemptAsync(failedAudit, cancellationToken);

            return new VerifyOtpResult { Success = false };
        }

        // Successful OTP: delegate to identity/patient provisioning service.
        var (user, patient) = await _identityOtpProvisioningService
            .EnsureUserAndPatientForPhoneAsync(request.PhoneNumber, cancellationToken);

        userIdForAudit = user.Id;

        var token = _tokenService.GeneratePatientToken(user, patient.Id);

        var successAudit = new LoginAudit
        {
            Id = Guid.NewGuid(),
            UserId = userIdForAudit,
            LoginTime = _clock.UtcNow,
            IpAddress = _currentUser.IpAddress,
            UserAgent = _currentUser.UserAgent,
            Success = true,
            CreatedAt = _clock.UtcNow
        };

        await _identityOtpProvisioningService.LogLoginAttemptAsync(successAudit, cancellationToken);

        return new VerifyOtpResult
        {
            Success = true,
            Token = token
        };
    }
}

