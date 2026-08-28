using RaphCare.Domain.Identity;

namespace RaphCare.Application.Common.Interfaces;

/// <summary>
/// Abstraction for finding and persisting application users (e.g. for Entra provisioning).
/// Implemented by Infrastructure; consumed by Identity layer.
/// </summary>
public interface IApplicationUserStore
{
    /// <summary>Finds a user by their Microsoft Entra object identifier.</summary>
    /// <param name="entraObjectId">The Entra ID oid claim value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user if found; otherwise null.</returns>
    Task<ApplicationUser?> FindByEntraObjectIdAsync(string entraObjectId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new application user.</summary>
    /// <param name="user">The user to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created user (with Id populated).</returns>
    Task<ApplicationUser> CreateAsync(ApplicationUser user, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing application user (e.g. after syncing from Entra).</summary>
    /// <param name="user">The user to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default);

    /// <summary>Finds a user by primary key.</summary>
    Task<ApplicationUser?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
