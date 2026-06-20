using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace RaphCare.Identity.LocalJwt;

internal static class LocalJwtTokenHelper
{
    internal static bool IsLocalEmailJwt(string? rawToken, SecurityToken? securityToken, LocalJwtOptions localJwt)
    {
        if (string.IsNullOrWhiteSpace(localJwt.Issuer) && string.IsNullOrWhiteSpace(localJwt.Secret))
            return false;

        if (securityToken is JwtSecurityToken jwt
            && string.Equals(jwt.Issuer, localJwt.Issuer, StringComparison.Ordinal))
            return true;

        if (string.IsNullOrWhiteSpace(rawToken))
            return false;

        var bearer = rawToken.Trim();
        if (bearer.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            bearer = bearer["Bearer ".Length..].Trim();

        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(bearer))
                return false;

            var parsed = handler.ReadJwtToken(bearer);
            return string.Equals(parsed.Issuer, localJwt.Issuer, StringComparison.Ordinal)
                   || string.Equals(parsed.SignatureAlgorithm, SecurityAlgorithms.HmacSha256, StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }

    internal static bool IsLocalEmailPrincipal(ClaimsPrincipal principal, LocalJwtOptions localJwt)
    {
        if (string.IsNullOrWhiteSpace(localJwt.Issuer))
            return false;

        var issuer = principal.FindFirst("iss")?.Value;
        if (string.Equals(issuer, localJwt.Issuer, StringComparison.Ordinal))
            return true;

        // Email/password JWTs use sub + role; Entra tokens always carry oid.
        return !HasEntraObjectId(principal)
               && principal.FindFirst("role") is not null;
    }

    internal static bool HasEntraObjectId(ClaimsPrincipal principal) =>
        !string.IsNullOrWhiteSpace(principal.FindFirstValue("oid")
            ?? principal.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier"));
}
