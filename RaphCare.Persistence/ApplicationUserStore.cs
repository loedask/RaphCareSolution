using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Identity;

namespace RaphCare.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IApplicationUserStore"/> using <see cref="IdentityDbContext"/>.
/// </summary>
public class ApplicationUserStore(IdentityDbContext context) : IApplicationUserStore
{
    private readonly IdentityDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <inheritdoc />
    public async Task<ApplicationUser?> FindByEntraObjectIdAsync(string entraObjectId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entraObjectId))
            return null;
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.EntraObjectId == entraObjectId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ApplicationUser> CreateAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return user;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ApplicationUser?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return null;

        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }
}
