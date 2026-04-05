using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RaphCare.Application.Common.Configuration;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Infrastructure.StandaloneEmergency;

/// <summary>HMAC-SHA256 over the raw body; header value is lowercase hex (64 chars), no prefix.</summary>
public sealed class StandaloneEmergencyWebhookSignatureValidator(
    IOptions<StandaloneEmergencyOptions> options,
    IHostEnvironment environment) : IStandaloneEmergencyWebhookSignatureValidator
{
    private readonly StandaloneEmergencyOptions _options = options.Value;
    private readonly IHostEnvironment _environment = environment;

    public bool IsValid(string? signatureHeader, ReadOnlySpan<byte> bodyUtf8)
    {
        var secret = _options.WebhookSharedSecret?.Trim();
        if (string.IsNullOrEmpty(secret))
        {
            if (_environment.IsDevelopment() && _options.AllowUnsignedWebhooksInDevelopment)
                return true;

            return false;
        }

        if (string.IsNullOrWhiteSpace(signatureHeader))
        {
            if (_environment.IsDevelopment() && _options.AllowUnsignedWebhooksInDevelopment)
                return true;

            return false;
        }

        var expectedBytes = ComputeHmacBytes(secret, bodyUtf8);
        var provided = signatureHeader.Trim();
        if (provided.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase))
            provided = provided["sha256=".Length..].Trim();

        byte[] providedBytes;
        try
        {
            providedBytes = Convert.FromHexString(provided);
        }
        catch (FormatException)
        {
            return false;
        }

        return providedBytes.Length == expectedBytes.Length
               && CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }

    private static byte[] ComputeHmacBytes(string secret, ReadOnlySpan<byte> bodyUtf8)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(bodyUtf8.ToArray());
    }
}
