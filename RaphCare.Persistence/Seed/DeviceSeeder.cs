using Microsoft.Extensions.Logging;

namespace RaphCare.Persistence.Seed;

/// <summary>
/// Seeds device bounded-context data. Placeholder for future device reference or link data.
/// Idempotent when implemented.
/// </summary>
public static class DeviceSeeder
{
    private static readonly Action<ILogger, Exception?> LogNotImplemented =
        LoggerMessage.Define(
            LogLevel.Debug,
            new EventId(1, nameof(LogNotImplemented)),
            "Device seeder: not yet implemented.");

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
        LogNotImplemented(logger, null);
        return Task.CompletedTask;
    }
}
