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

    private static readonly Action<ILogger, Exception?> LogDemoProviderSeeded =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(3, nameof(LogDemoProviderSeeded)),
            "Demo telehealth provider seeded.");

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

        var anyClinics = await context.Clinics.AnyAsync(cancellationToken).ConfigureAwait(false);
        if (!anyClinics)
        {
            var clinic = new Clinic
            {
                Name = "RaphCare Demo Clinic",
                RegistrationNumber = "REG-DEMO-001",
                Country = "South Africa",
                TimeZone = "South Africa Standard Time",
                IsActive = true
            };
            context.Clinics.Add(clinic);
            context.Entry(clinic).Property(c => c.Id).CurrentValue = ClinicalSeedIds.DemoClinicId;
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            LogExampleClinicSeeded(logger, null);
        }
        else
        {
            LogClinicAlreadySeeded(logger, null);
        }

        await EnsureDemoTelehealthProviderAsync(context, logger, cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsureDemoTelehealthProviderAsync(
        ClinicalDbContext context,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var providers = context.Set<Provider>();
        if (await providers.AnyAsync(p => p.IsActive && !p.IsDeleted, cancellationToken).ConfigureAwait(false))
            return;

        var clinicId = await context.Clinics
            .Where(c => c.IsActive)
            .OrderBy(c => c.Id == ClinicalSeedIds.DemoClinicId ? 0 : 1)
            .ThenBy(c => c.CreatedAt)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (clinicId == Guid.Empty)
            return;

        var provider = new Provider
        {
            ClinicId = clinicId,
            ApplicationUserId = ClinicalSeedIds.DemoProviderApplicationUserId,
            LicenseNumber = "DEMO-LIC-001",
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        };
        providers.Add(provider);
        context.Entry(provider).Property(p => p.Id).CurrentValue = ClinicalSeedIds.DemoProviderId;
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        LogDemoProviderSeeded(logger, null);
    }
}
