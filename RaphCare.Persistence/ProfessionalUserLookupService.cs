using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Identity;
using RaphCare.Domain.Identity;

namespace RaphCare.Persistence;

public sealed class ProfessionalUserLookupService(IdentityDbContext identityDbContext) : IProfessionalUserLookupService
{
    public async Task<ApplicationUser?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
            return null;

        return await identityDbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == normalized && u.IsActive && !u.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<ApplicationUser>> GetUsersByIdsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0)
            return Array.Empty<ApplicationUser>();

        return await identityDbContext.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> IsProfessionalAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await identityDbContext.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(identityDbContext.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r.Name)
            .AnyAsync(name => name == RaphCareRoles.Clinician || name == RaphCareRoles.Administrator, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> HasSuccessfulLoginAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await identityDbContext.LoginAudits
            .AsNoTracking()
            .AnyAsync(a => a.UserId == userId && a.Success, cancellationToken)
            .ConfigureAwait(false);
    }
}
