using RaphCare.Domain.Identity;

namespace RaphCare.Application.Common.Interfaces;

/// <summary>
/// Ensures a domain ApplicationUser exists for the given principal (e.g. after Entra authentication).
/// Implemented by RaphCare.Identity.
/// </summary>
public interface IUserProvisioningService
{
    Task<ApplicationUser> EnsureUserExistsAsync(System.Security.Claims.ClaimsPrincipal principal, CancellationToken cancellationToken = default);
}
