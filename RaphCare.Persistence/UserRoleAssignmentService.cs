using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Identity;

namespace RaphCare.Persistence;

public sealed class UserRoleAssignmentService(IdentityDbContext identityDbContext) : IUserRoleAssignmentService
{
    public async Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var roles = await identityDbContext.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(identityDbContext.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return roles;
    }

    public async Task AssignRoleIfMissingAsync(Guid userId, string roleName, CancellationToken cancellationToken = default)
    {
        var role = await identityDbContext.Roles
            .FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken)
            .ConfigureAwait(false);
        if (role is null)
            return;

        var exists = await identityDbContext.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == role.Id, cancellationToken)
            .ConfigureAwait(false);
        if (exists)
            return;

        identityDbContext.UserRoles.Add(new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoleId = role.Id,
            CreatedAt = DateTime.UtcNow
        });
        await identityDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
