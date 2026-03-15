using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Identity;
using RaphCare.Persistence;

namespace RaphCare.Application.Features.Auth.Commands.VerifyOtp;

public class VerifyOtpHandler : IRequestHandler<VerifyOtpCommand, bool>
{
    private readonly IOtpService _otpService;
    private readonly IdentityDbContext _dbContext;
    private readonly IDateTimeProvider _clock;
    private readonly ICurrentUserService _currentUser;

    public VerifyOtpHandler(
        IOtpService otpService,
        IdentityDbContext dbContext,
        IDateTimeProvider clock,
        ICurrentUserService currentUser)
    {
        _otpService = otpService;
        _dbContext = dbContext;
        _clock = clock;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        var success = await _otpService.ValidateOtpAsync(request.PhoneNumber, request.Code, cancellationToken);

        // LoginAudit is tied to ApplicationUser; for phone onboarding there may be no user yet.
        // We log with a null user by convention (UserId = Guid.Empty).
        var loginAudit = new LoginAudit
        {
            Id = Guid.NewGuid(),
            UserId = Guid.Empty,
            LoginTime = _clock.UtcNow,
            IpAddress = _currentUser.IpAddress,
            UserAgent = _currentUser.UserAgent,
            Success = success,
            CreatedAt = _clock.UtcNow
        };

        _dbContext.Add(loginAudit);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return success;
    }
}

