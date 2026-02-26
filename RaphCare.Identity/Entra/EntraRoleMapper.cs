using System.Security.Claims;

namespace RaphCare.Identity.Entra;

/// <summary>
/// Maps Entra roles/groups from the JWT to internal role names.
/// Uses "roles" or "groups" claim types.
/// </summary>
public class EntraRoleMapper
{
    private const string RolesClaimType = "roles";
    private const string GroupsClaimType = "groups";

    /// <summary>
    /// Returns the set of internal role names for the principal (e.g. for authorization).
    /// </summary>
    public static IEnumerable<string> MapRoles(ClaimsPrincipal principal)
    {
        if (principal == null)
            yield break;

        var roles = principal.FindAll(RolesClaimType).Select(c => c.Value).Where(v => !string.IsNullOrWhiteSpace(v));
        foreach (var role in roles)
            yield return role!;

        var groups = principal.FindAll(GroupsClaimType).Select(c => c.Value).Where(v => !string.IsNullOrWhiteSpace(v));
        foreach (var group in groups)
            yield return group!;
    }
}
