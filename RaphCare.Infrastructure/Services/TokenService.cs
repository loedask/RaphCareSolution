using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Identity;

namespace RaphCare.Infrastructure.Services;

/// <summary>Generates JWT tokens for patient sessions.</summary>
public class TokenService(IOptions<JwtOptions> options) : ITokenService
{
    private readonly JwtOptions _options = options.Value;

    public string GeneratePatientToken(ApplicationUser user, Guid patientId)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("patientId", patientId.ToString()),
            new("phone", user.Email ?? string.Empty),
            new(ClaimTypes.Role, "Patient")
        };

        var key = CreateSigningKey(_options.Secret);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateStaffToken(ApplicationUser user, IReadOnlyList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("name", user.DisplayName ?? string.Empty)
        };

        foreach (var role in roles.Where(r => !string.IsNullOrWhiteSpace(r)).Distinct(StringComparer.OrdinalIgnoreCase))
            claims.Add(new Claim(ClaimTypes.Role, role));

        var key = CreateSigningKey(_options.Secret);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>HS256 requires at least 256 bits; hash shorter configured secrets to a 256-bit key.</summary>
    internal static SymmetricSecurityKey CreateSigningKey(string secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
            throw new InvalidOperationException("Jwt:Secret is not configured.");

        var secretBytes = Encoding.UTF8.GetBytes(secret);
        var keyBytes = secretBytes.Length >= 32 ? secretBytes : SHA256.HashData(secretBytes);
        return new SymmetricSecurityKey(keyBytes);
    }
}
