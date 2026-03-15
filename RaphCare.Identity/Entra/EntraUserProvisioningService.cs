using System.Security.Claims;
using Microsoft.Extensions.Logging;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Identity;

namespace RaphCare.Identity.Entra;

/// <summary>
/// Ensures an ApplicationUser exists for the Entra principal and syncs EntraObjectId, Email, DisplayName. Role in authentication pipeline: call after JWT validation to provision or sync the domain user for the current request.
/// </summary>
public class EntraUserProvisioningService(
    IApplicationUserStore userStore,
    ILogger<EntraUserProvisioningService> logger
    ) : IUserProvisioningService
{
    private readonly IApplicationUserStore _userStore = userStore ?? throw new ArgumentNullException(nameof(userStore));
    private readonly ILogger<EntraUserProvisioningService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>Ensures a domain user exists for the principal; creates or updates from Entra claims (oid, email, name).</summary>
    /// <param name="principal">The authenticated claims principal from the JWT.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The application user (existing or newly created).</returns>
    public async Task<ApplicationUser> EnsureUserExistsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var entraObjectId = GetEntraObjectId(principal);
        if (string.IsNullOrWhiteSpace(entraObjectId))
            throw new InvalidOperationException("Entra object ID (oid) claim is missing.");

        var existing = await _userStore.FindByEntraObjectIdAsync(entraObjectId, cancellationToken).ConfigureAwait(false);
        if (existing != null)
        {
            await SyncUserAsync(existing, principal, cancellationToken).ConfigureAwait(false);
            return existing;
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            EntraObjectId = entraObjectId,
            Email = GetEmail(principal),
            DisplayName = GetDisplayName(principal),
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        await _userStore.CreateAsync(user, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Provisioned new user from Entra: {EntraObjectId}, {Email}", user.EntraObjectId, user.Email);
        return user;
    }

    private async Task SyncUserAsync(ApplicationUser user, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        user.Email = GetEmail(principal);
        user.DisplayName = GetDisplayName(principal);
        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _userStore.UpdateAsync(user, cancellationToken).ConfigureAwait(false);
    }

    private static string GetEntraObjectId(ClaimsPrincipal principal)
    {
        return principal.FindFirstValue("oid")
               ?? principal.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
               ?? string.Empty;
    }

    private static string GetEmail(ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Email)
               ?? principal.FindFirstValue("email")
               ?? principal.FindFirstValue("preferred_username")
               ?? string.Empty;
    }

    private static string GetDisplayName(ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Name)
               ?? principal.FindFirstValue("name")
               ?? string.Empty;
    }
}
