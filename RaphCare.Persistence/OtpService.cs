using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Identity;

namespace RaphCare.Persistence;

/// <summary>OTP generation and validation backed by the identity database.</summary>
public class OtpService(IdentityDbContext dbContext, IDateTimeProvider clock) : IOtpService
{
    private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(10);
    private const int MaxOtpsPerWindow = 3;

    private readonly IdentityDbContext _dbContext = dbContext;
    private readonly IDateTimeProvider _clock = clock;

    public async Task<string> GenerateOtpAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;
        var windowStart = now - RateLimitWindow;

        var recentCount = await _dbContext.OtpCodes
            .Where(x => x.PhoneNumber == phoneNumber && x.CreatedAt >= windowStart)
            .CountAsync(cancellationToken);

        if (recentCount >= MaxOtpsPerWindow)
        {
            throw new InvalidOperationException("OTP request limit exceeded. Please try again later.");
        }

        var code = GenerateSixDigitCode();
        var hash = HashCode(code);

        var entity = new OtpCode
        {
            Id = Guid.NewGuid(),
            PhoneNumber = phoneNumber,
            CodeHash = hash,
            ExpiresAt = now.Add(OtpLifetime),
            IsUsed = false,
            CreatedAt = now
        };

        _dbContext.OtpCodes.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return code;
    }

    public async Task<bool> ValidateOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;
        var hash = HashCode(code);

        var otp = await _dbContext.OtpCodes
            .Where(x => x.PhoneNumber == phoneNumber && x.CodeHash == hash)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (otp == null) return false;
        if (otp.IsUsed) return false;
        if (otp.ExpiresAt <= now) return false;

        otp.IsUsed = true;
        otp.UsedAt = now;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static string GenerateSixDigitCode()
    {
        var bytes = RandomNumberGenerator.GetBytes(4);
        var value = BitConverter.ToUInt32(bytes, 0) % 1_000_000;
        return value.ToString("D6");
    }

    private static string HashCode(string code)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(code);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }
}

