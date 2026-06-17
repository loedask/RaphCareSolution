using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RaphCare.Domain.Insurance;
using RaphCare.Persistence;

namespace RaphCare.Persistence.Seed;

/// <summary>
/// Seeds insurance bounded-context data: insurance plans.
/// Idempotent: skips if any insurance plans already exist.
/// </summary>
public static class InsuranceSeeder
{
    /// <summary>
    /// Seeds a sample insurance plan when none exist.
    /// </summary>
    /// <param name="scopedProvider">Scoped service provider (e.g. from CreateAsyncScope).</param>
    /// <param name="logger">Logger for this seeder.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task SeedAsync(
        IServiceProvider scopedProvider,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var context = scopedProvider.GetService<InsuranceDbContext>();
        if (context == null) return;

        if (await context.InsurancePlans.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            logger.LogInformation("Insurance plans already seeded.");
            return;
        }

        var plan = new InsurancePlan
        {
            Name = "Standard Medical Aid",
            Code = "STD-001",
            Description = "Sample insurance plan for seeding",
            IsActive = true
        };
        context.InsurancePlans.Add(plan);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Sample insurance plan seeded.");
    }
}
