using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RaphCare.Domain.Organization;
using RaphCare.Persistence;

namespace RaphCare.Persistence.Seed;

/// <summary>
/// Seeds clinical bounded-context data: clinics and related reference data.
/// Idempotent: skips if any clinics already exist.
/// </summary>
public static class ClinicalSeeder
{
    private static readonly Action<ILogger, Exception?> LogClinicAlreadySeeded =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(1, nameof(LogClinicAlreadySeeded)),
            "Clinic already seeded.");

    private static readonly Action<ILogger, Exception?> LogExampleClinicSeeded =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(2, nameof(LogExampleClinicSeeded)),
            "Example clinic seeded.");

    /// <summary>
    /// Seeds a default demo clinic when none exist.
    /// </summary>
    /// <param name="scopedProvider">Scoped service provider (e.g. from CreateAsyncScope).</param>
    /// <param name="logger">Logger for this seeder.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task SeedAsync(
        IServiceProvider scopedProvider,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var context = scopedProvider.GetService<ClinicalDbContext>();
        if (context == null) return;

        if (await context.Clinics.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            LogClinicAlreadySeeded(logger, null);
            return;
        }

        var clinic = new Clinic
        {
            Name = "RaphCare Demo Clinic",
            RegistrationNumber = "REG-DEMO-001",
            Country = "South Africa",
            TimeZone = "South Africa Standard Time",
            IsActive = true
        };
        context.Clinics.Add(clinic);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        LogExampleClinicSeeded(logger, null);
    }
}
