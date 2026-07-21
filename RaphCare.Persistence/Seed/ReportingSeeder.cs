using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RaphCare.Domain.Reporting;
using RaphCare.Persistence;

namespace RaphCare.Persistence.Seed;

/// <summary>Seeds a demo reporting dashboard snapshot for local demos.</summary>
public static class ReportingSeeder
{
    private static readonly Action<ILogger, Exception?> LogSnapshotSeeded =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(1, nameof(LogSnapshotSeeded)),
            "Demo dashboard snapshot seeded.");

    private static readonly Action<ILogger, Exception?> LogSnapshotExists =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(2, nameof(LogSnapshotExists)),
            "Dashboard snapshot already present.");

    public static async Task SeedAsync(
        IServiceProvider scopedProvider,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var context = scopedProvider.GetService<AIDbContext>();
        if (context is null)
            return;

        var clinicId = ClinicalSeedIds.DemoClinicId;
        var exists = await context.DashboardSnapshots
            .AnyAsync(s => s.ClinicId == clinicId, cancellationToken)
            .ConfigureAwait(false);
        if (exists)
        {
            LogSnapshotExists(logger, null);
            return;
        }

        var snapshotDate = DateTime.UtcNow.Date;
        var json =
            "{"
            + $"\"clinicId\":\"{clinicId}\","
            + $"\"snapshotDate\":\"{snapshotDate:yyyy-MM-dd}\","
            + "\"occupancy\":{\"beds\":12,\"available\":7,\"occupied\":4,\"maintenance\":1},"
            + "\"admissions\":{\"active\":4,\"dischargedLast7Days\":6},"
            + "\"appointments\":{\"today\":18,\"upcoming7Days\":64},"
            + "\"telehealth\":{\"sessionsToday\":3},"
            + "\"devices\":{\"activeWearables\":9},"
            + "\"generatedBy\":\"ReportingSeeder\""
            + "}";

        context.DashboardSnapshots.Add(new DashboardSnapshot
        {
            ClinicId = clinicId,
            SnapshotDate = snapshotDate,
            SnapshotJson = json
        });
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        LogSnapshotSeeded(logger, null);
    }
}
