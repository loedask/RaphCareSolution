using RaphCare.Domain.Identity;

namespace RaphCare.Application.Common.Interfaces;

/// <summary>
/// Abstraction for finding and persisting application users (e.g. for Entra provisioning).
/// Implemented by Infrastructure; consumed by Identity layer.
/// </summary>
public interface IApplicationUserStore
{
    Task<ApplicationUser?> FindByEntraObjectIdAsync(string entraObjectId, CancellationToken cancellationToken = default);

    Task<ApplicationUser> CreateAsync(ApplicationUser user, CancellationToken cancellationToken = default);

    Task UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default);
}
