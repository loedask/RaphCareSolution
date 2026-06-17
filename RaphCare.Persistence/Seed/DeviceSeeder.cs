using Microsoft.Extensions.Logging;

namespace RaphCare.Persistence.Seed;

/// <summary>
/// Seeds device bounded-context data. Placeholder for future device reference or link data.
/// Idempotent when implemented.
/// </summary>
public static class DeviceSeeder
{
    /// <summary>
    /// Seeds device-related data when none exist. Currently a no-op with logging placeholder.
    /// </summary>
    /// <param name="scopedProvider">Scoped service provider (e.g. from CreateAsyncScope).</param>
    /// <param name="logger">Logger for this seeder.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static Task SeedAsync(
        IServiceProvider scopedProvider,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Device seeder: not yet implemented.");
        return Task.CompletedTask;
    }
}
