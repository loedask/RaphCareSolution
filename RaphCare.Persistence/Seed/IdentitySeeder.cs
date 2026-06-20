using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RaphCare.Domain.Identity;
using RaphCare.Persistence;

namespace RaphCare.Persistence.Seed;

/// <summary>
/// Seeds identity bounded-context data: roles and permissions.
/// Idempotent: ensures default roles and permissions exist by name.
/// </summary>
public static class IdentitySeeder
{
    private static readonly (string Name, string Description)[] DefaultRoles =
    [
        ("Administrator", "Full system access"),
        ("Clinician", "Clinical access"),
        ("Patient", "Patient portal access")
    ];

    private static readonly (string Code, string Name)[] DefaultPermissions =
    [
        ("Patients.Read", "View patients"),
        ("Patients.Write", "Edit patients"),
        ("Visits.Read", "View visits"),
        ("Visits.Write", "Document visits"),
        ("Admin.All", "Administrative access")
    ];

    /// <summary>
    /// Ensures default roles and permissions exist.
    /// </summary>
    public static async Task SeedAsync(
        IServiceProvider scopedProvider,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var context = scopedProvider.GetService<IdentityDbContext>();
        if (context is null)
            return;

        var now = DateTime.UtcNow;
        var existingRoles = await context.Roles
            .AsNoTracking()
            .Select(r => r.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var (name, description) in DefaultRoles)
        {
            if (existingRoles.Contains(name, StringComparer.OrdinalIgnoreCase))
                continue;

            context.Roles.Add(new Role
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                CreatedAt = now
            });
            logger.LogInformation("Seeded role {RoleName}.", name);
        }

        var existingPermissions = await context.Permissions
            .AsNoTracking()
            .Select(p => p.Code)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var (code, name) in DefaultPermissions)
        {
            if (existingPermissions.Contains(code, StringComparer.OrdinalIgnoreCase))
                continue;

            context.Permissions.Add(new Permission
            {
                Id = Guid.NewGuid(),
                Code = code,
                Name = name,
                CreatedAt = now
            });
            logger.LogInformation("Seeded permission {PermissionCode}.", code);
        }

        if (context.ChangeTracker.HasChanges())
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
