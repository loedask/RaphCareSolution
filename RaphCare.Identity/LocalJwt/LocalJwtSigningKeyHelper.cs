using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace RaphCare.Identity.LocalJwt;

internal static class LocalJwtSigningKeyHelper
{
    internal static SymmetricSecurityKey CreateSigningKey(string secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
            throw new InvalidOperationException("Jwt:Secret is not configured.");

        var secretBytes = Encoding.UTF8.GetBytes(secret);
        var keyBytes = secretBytes.Length >= 32 ? secretBytes : SHA256.HashData(secretBytes);
        return new SymmetricSecurityKey(keyBytes);
    }
}
