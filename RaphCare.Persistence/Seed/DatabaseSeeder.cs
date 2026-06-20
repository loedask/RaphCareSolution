using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RaphCare.Persistence.Seed;

/// <summary>
/// Orchestrates database seeding across bounded contexts. Creates a single async scope and invokes
/// each context-specific seeder in order. Does not contain seeding logic directly.
/// </summary>
public static class DatabaseSeeder
{
    private static readonly Action<ILogger, Exception?> LogSeedError =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(1, nameof(LogSeedError)),
            "An error occurred while seeding the database.");

    /// <summary>
    /// Runs all seeders (Identity, Clinical, Insurance, Device, Billing) within one async scope.
    /// Logs and rethrows any exception after logging.
    /// </summary>
    /// <param name="serviceProvider">Root service provider (e.g. from host or app).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("RaphCare.Persistence.Seed.DatabaseSeeder");
        try
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var scopedProvider = scope.ServiceProvider;

            await IdentitySeeder.SeedAsync(scopedProvider, logger, cancellationToken).ConfigureAwait(false);
            await ClinicalSeeder.SeedAsync(scopedProvider, logger, cancellationToken).ConfigureAwait(false);
            await InsuranceSeeder.SeedAsync(scopedProvider, logger, cancellationToken).ConfigureAwait(false);
            await DeviceSeeder.SeedAsync(scopedProvider, logger, cancellationToken).ConfigureAwait(false);
            await BillingSeeder.SeedAsync(scopedProvider, logger, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogSeedError(logger, ex);
            throw;
        }
    }
}
