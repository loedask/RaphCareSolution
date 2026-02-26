using RaphCare.Domain.Identity;

namespace RaphCare.Application.Common.Interfaces;

/// <summary>
/// Ensures a domain ApplicationUser exists for the given principal (e.g. after Entra authentication).
/// Implemented by RaphCare.Identity.
/// </summary>
public interface IUserProvisioningService
{
    /// <summary>Ensures a domain user exists for the authenticated principal; creates or updates from Entra claims.</summary>
    /// <param name="principal">The authenticated claims principal (e.g. from JWT).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The application user (existing or newly created).</returns>
    Task<ApplicationUser> EnsureUserExistsAsync(System.Security.Claims.ClaimsPrincipal principal, CancellationToken cancellationToken = default);
}
