using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Persistence;

/// <summary>Email verification OTP generation and validation backed by the identity database.</summary>
public sealed class EmailOtpService(IdentityDbContext dbContext, IDateTimeProvider clock) : IEmailOtpService
{
    private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(10);
    private const int MaxOtpsPerWindow = 5;

    public async Task<string> GenerateOtpAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeEmail(email);
        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException("Email is required.", nameof(email));

        var now = clock.UtcNow;
        var windowStart = now - RateLimitWindow;

        var recentCount = await dbContext.OtpCodes
            .Where(x => x.Email == normalized && x.CreatedAt >= windowStart)
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        if (recentCount >= MaxOtpsPerWindow)
            throw new InvalidOperationException("Verification email limit exceeded. Please try again later.");

        var code = GenerateSixDigitCode();
        var hash = HashCode(code);

        dbContext.OtpCodes.Add(new Domain.Identity.OtpCode
        {
            Id = Guid.NewGuid(),
            Email = normalized,
            CodeHash = hash,
            ExpiresAt = now.Add(OtpLifetime),
            IsUsed = false,
            CreatedAt = now
        });
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return code;
    }

    public async Task<bool> ValidateOtpAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeEmail(email);
        var now = clock.UtcNow;
        var hash = HashCode(code);

        var otp = await dbContext.OtpCodes
            .Where(x => x.Email == normalized && x.CodeHash == hash)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (otp is null || otp.IsUsed || otp.ExpiresAt <= now)
            return false;

        otp.IsUsed = true;
        otp.UsedAt = now;
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static string GenerateSixDigitCode()
    {
        var bytes = RandomNumberGenerator.GetBytes(4);
        var value = BitConverter.ToUInt32(bytes, 0) % 1_000_000;
        return value.ToString("D6");
    }

    private static string HashCode(string code)
    {
        var bytes = Encoding.UTF8.GetBytes(code);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
