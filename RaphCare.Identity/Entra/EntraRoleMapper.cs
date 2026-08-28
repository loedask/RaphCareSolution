using System.Security.Claims;
using RaphCare.Domain.Identity;

namespace RaphCare.Identity.Entra;

/// <summary>
/// Maps Entra app roles (and known aliases) to seeded RaphCare role names.
/// </summary>
public class EntraRoleMapper
{
    private const string RolesClaimType = "roles";
    private const string GroupsClaimType = "groups";

    /// <summary>
    /// Returns canonical RaphCare role names present on the principal.
    /// Unknown Entra group IDs are ignored so they cannot become authorization roles.
    /// </summary>
    public static IEnumerable<string> MapRoles(ClaimsPrincipal principal)
    {
        if (principal == null)
            yield break;

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var raw in ReadRawRoleValues(principal))
        {
            var mapped = RaphCareRoles.MapExternalRole(raw);
            if (mapped is null || !seen.Add(mapped))
                continue;
            yield return mapped;
        }
    }

    /// <summary>Copies canonical roles onto <see cref="ClaimTypes.Role"/> so <c>[Authorize]</c> policies can see them.</summary>
    public static void ApplyAuthorizationRoleClaims(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity)
            return;

        foreach (var role in MapRoles(principal))
        {
            if (!identity.HasClaim(ClaimTypes.Role, role))
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }
    }

    private static IEnumerable<string> ReadRawRoleValues(ClaimsPrincipal principal)
    {
        foreach (var claim in principal.FindAll(RolesClaimType))
        {
            if (!string.IsNullOrWhiteSpace(claim.Value))
                yield return claim.Value;
        }

        foreach (var claim in principal.FindAll(ClaimTypes.Role))
        {
            if (!string.IsNullOrWhiteSpace(claim.Value))
                yield return claim.Value;
        }

        foreach (var claim in principal.FindAll("role"))
        {
            if (!string.IsNullOrWhiteSpace(claim.Value))
                yield return claim.Value;
        }

        foreach (var claim in principal.FindAll(GroupsClaimType))
        {
            if (!string.IsNullOrWhiteSpace(claim.Value))
                yield return claim.Value;
        }
    }
}
