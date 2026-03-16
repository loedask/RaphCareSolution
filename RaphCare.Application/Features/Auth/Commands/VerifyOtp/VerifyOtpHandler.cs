using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Identity;
using RaphCare.Domain.Patients;
using RaphCare.Persistence;

namespace RaphCare.Application.Features.Auth.Commands.VerifyOtp;

public class VerifyOtpHandler : IRequestHandler<VerifyOtpCommand, VerifyOtpResult>
{
    private readonly IOtpService _otpService;
    private readonly IdentityDbContext _dbContext;
    private readonly ClinicalDbContext _clinicalDbContext;
    private readonly IDateTimeProvider _clock;
    private readonly ICurrentUserService _currentUser;
    private readonly ITokenService _tokenService;

    public VerifyOtpHandler(
        IOtpService otpService,
        IdentityDbContext dbContext,
        ClinicalDbContext clinicalDbContext,
        IDateTimeProvider clock,
        ICurrentUserService currentUser,
        ITokenService tokenService)
    {
        _otpService = otpService;
        _dbContext = dbContext;
        _clinicalDbContext = clinicalDbContext;
        _clock = clock;
        _currentUser = currentUser;
        _tokenService = tokenService;
    }

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

            _dbContext.Add(failedAudit);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new VerifyOtpResult { Success = false };
        }

        // Successful OTP: find or create ApplicationUser for this phone-based patient.
        // We use Email to store the phone identifier in this first iteration.
        var normalizedPhone = request.PhoneNumber.Trim();

        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(
                u => u.Email == normalizedPhone,
                cancellationToken);

        if (user == null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                EntraObjectId = string.Empty,
                Email = normalizedPhone,
                DisplayName = normalizedPhone,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = _clock.UtcNow
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        userIdForAudit = user.Id;

        // Ensure Patient entity exists and is linked to this ApplicationUser.
        var patient = await _clinicalDbContext.Patients
            .FirstOrDefaultAsync(p => p.ApplicationUserId == user.Id, cancellationToken);

        if (patient is null)
        {
            var normalizedPhone = request.PhoneNumber.Trim();

            // Try to backfill an existing patient created via other flows using the same phone number.
            patient = await _clinicalDbContext.Patients
                .FirstOrDefaultAsync(p => p.PhoneNumber == normalizedPhone, cancellationToken);

            if (patient is null)
            {
                patient = new Patient
                {
                    ClinicId = Guid.Empty,
                    FirstName = normalizedPhone,
                    LastName = string.Empty,
                    DateOfBirth = DateTime.UtcNow, // placeholder until onboarding collects real DOB
                    PhoneNumber = normalizedPhone,
                    IsActive = true
                };

                await _clinicalDbContext.Patients.AddAsync(patient, cancellationToken);
            }

            patient.LinkToApplicationUser(user.Id);
            await _clinicalDbContext.SaveChangesAsync(cancellationToken);
        }

        // Ensure Patient role is assigned.
        var patientRole = await _dbContext.Roles
            .FirstOrDefaultAsync(r => r.Name == "Patient", cancellationToken);

        if (patientRole != null)
        {
            var hasPatientRole = await _dbContext.UserRoles
                .AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == patientRole.Id, cancellationToken);

            if (!hasPatientRole)
            {
                var link = new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    RoleId = patientRole.Id,
                    CreatedAt = _clock.UtcNow
                };

                _dbContext.UserRoles.Add(link);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

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

        _dbContext.Add(successAudit);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new VerifyOtpResult
        {
            Success = true,
            Token = token
        };
    }
}

