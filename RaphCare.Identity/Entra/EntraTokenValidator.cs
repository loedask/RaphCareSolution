using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace RaphCare.Identity.Entra;

/// <summary>
/// Validates JWTs issued by Microsoft Entra ID and returns a ClaimsPrincipal. Used in the authentication pipeline when validating bearer tokens (e.g. for API or background calls).
/// </summary>
public class EntraTokenValidator(Microsoft.Extensions.Options.IOptions<EntraOptions> options)
{
    private readonly JwtSecurityTokenHandler _tokenHandler = new();
    private readonly EntraOptions _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

    /// <summary>
    /// Validates the JWT and returns the claims principal. Throws on invalid token.
    /// </summary>
    public System.Security.Claims.ClaimsPrincipal ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));

        var parameters = BuildValidationParameters();
        var principal = _tokenHandler.ValidateToken(token, parameters, out _);
        return principal;
    }

    private TokenValidationParameters BuildValidationParameters()
    {
        var authority = _options.GetAuthority();
        var validIssuers = _options.ValidIssuers?.Length > 0
            ? _options.ValidIssuers
            : new[] { $"{authority}/", authority };

        return new TokenValidationParameters
        {
            ValidAudience = _options.Audience,
            ValidAudiences = null,
            ValidIssuer = null,
            ValidIssuers = validIssuers,
            ValidateIssuer = true,
            ValidateAudience = !string.IsNullOrWhiteSpace(_options.Audience),
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(2),
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            {
                var metadataAddress = $"{authority}/.well-known/openid-configuration";
                var configManager = new Microsoft.IdentityModel.Protocols.ConfigurationManager<Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration>(
                    metadataAddress,
                    new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfigurationRetriever());
                var config = configManager.GetConfigurationAsync(CancellationToken.None).GetAwaiter().GetResult();
                return config.SigningKeys;
            }
        };
    }
}
