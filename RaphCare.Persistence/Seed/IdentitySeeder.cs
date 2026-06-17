using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RaphCare.Domain.Identity;
using RaphCare.Persistence;

namespace RaphCare.Persistence.Seed;

/// <summary>
/// Seeds identity bounded-context data: roles and permissions.
/// Idempotent: skips if any roles already exist.
/// </summary>
public static class IdentitySeeder
{
    /// <summary>
    /// Seeds default roles and permissions when none exist.
    /// </summary>
    /// <param name="scopedProvider">Scoped service provider (e.g. from CreateAsyncScope).</param>
    /// <param name="logger">Logger for this seeder.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task SeedAsync(
        IServiceProvider scopedProvider,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var context = scopedProvider.GetService<IdentityDbContext>();
        if (context == null) return;

        if (await context.Roles.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            logger.LogInformation("Roles already seeded.");
            return;
        }

        var now = DateTime.UtcNow;

        var adminRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Administrator",
            Description = "Full system access",
            CreatedAt = now
        };
        var clinicianRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Clinician",
            Description = "Clinical access",
            CreatedAt = now
        };
        var patientRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Patient",
            Description = "Patient portal access",
            CreatedAt = now
        };

        context.Roles.AddRange(adminRole, clinicianRole, patientRole);

        var permissions = new[]
        {
            new Permission { Id = Guid.NewGuid(), Code = "Patients.Read", Name = "View patients", CreatedAt = now },
            new Permission { Id = Guid.NewGuid(), Code = "Patients.Write", Name = "Edit patients", CreatedAt = now },
            new Permission { Id = Guid.NewGuid(), Code = "Visits.Read", Name = "View visits", CreatedAt = now },
            new Permission { Id = Guid.NewGuid(), Code = "Visits.Write", Name = "Document visits", CreatedAt = now },
            new Permission { Id = Guid.NewGuid(), Code = "Admin.All", Name = "Administrative access", CreatedAt = now }
        };
        context.Permissions.AddRange(permissions);

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Roles and permissions seeded.");
    }
}
