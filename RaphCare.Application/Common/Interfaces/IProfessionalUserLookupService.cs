using RaphCare.Domain.Identity;

namespace RaphCare.Application.Common.Interfaces;

public interface IProfessionalUserLookupService
{
    Task<ApplicationUser?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApplicationUser>> GetUsersByIdsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken = default);
    Task<bool> IsProfessionalAsync(Guid userId, CancellationToken cancellationToken = default);
}
