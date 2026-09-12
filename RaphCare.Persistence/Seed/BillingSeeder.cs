using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RaphCare.Domain.Billing;

namespace RaphCare.Persistence.Seed;

/// <summary>
/// Seeds the commercial price catalog from price list v2. Idempotent by <see cref="PriceCatalogItem.SkuCode"/>.
/// </summary>
public static class BillingSeeder
{
    private static readonly Action<ILogger, int, Exception?> LogUpserted =
        LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(1, nameof(LogUpserted)),
            "Billing seeder: upserted {Count} price catalog SKU(s).");

    private static readonly (string Sku, string Name, string Category, decimal Zar, decimal Usd)[] Defaults =
    [
        (PriceCatalogSku.SitePractice, "Practice", PriceCatalogSku.Categories.Site, 1990m, 49m),
        (PriceCatalogSku.SiteClinic, "Clinic", PriceCatalogSku.Categories.Site, 3490m, 89m),
        (PriceCatalogSku.SiteHospital, "Hospital", PriceCatalogSku.Categories.Site, 9990m, 249m),
        (PriceCatalogSku.SiteNetwork, "Network", PriceCatalogSku.Categories.Site, 19990m, 499m),
        (PriceCatalogSku.SeatExtra, "Extra seat", PriceCatalogSku.Categories.Seat, 249m, 8m),
        (PriceCatalogSku.CareEssential, "Essential Care", PriceCatalogSku.Categories.PatientCare, 149m, 6m),
        (PriceCatalogSku.CareComplete, "Complete Care", PriceCatalogSku.Categories.PatientCare, 299m, 12m),
        (PriceCatalogSku.PkgHealthTrack, "Health Track", PriceCatalogSku.Categories.WatchPackage, 189m, 9m),
        (PriceCatalogSku.PkgSafeCare, "SafeCare", PriceCatalogSku.Categories.WatchPackage, 399m, 18m),
        (PriceCatalogSku.UsageVideoExtra, "Extra video", PriceCatalogSku.Categories.Usage, 89m, 4m),
        (PriceCatalogSku.AddonClinicAi, "Clinic AI add-on", PriceCatalogSku.Categories.Addon, 1490m, 39m),
    ];

    public static async Task SeedAsync(
        IServiceProvider scopedProvider,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var db = scopedProvider.GetRequiredService<BillingDbContext>();
        var changed = 0;

        foreach (var row in Defaults)
        {
            var existing = await db.PriceCatalogItems
                .FirstOrDefaultAsync(i => i.SkuCode == row.Sku, cancellationToken)
                .ConfigureAwait(false);

            if (existing is null)
            {
                await db.PriceCatalogItems.AddAsync(
                    new PriceCatalogItem
                    {
                        SkuCode = row.Sku,
                        DisplayName = row.Name,
                        Category = row.Category,
                        AmountZar = row.Zar,
                        AmountUsd = row.Usd,
                        IsActive = true
                    },
                    cancellationToken).ConfigureAwait(false);
                changed++;
                continue;
            }

            // Keep Ops-edited amounts. Only fill display metadata if somehow blank.
            var touch = false;
            if (string.IsNullOrWhiteSpace(existing.DisplayName))
            {
                existing.DisplayName = row.Name;
                touch = true;
            }

            if (string.IsNullOrWhiteSpace(existing.Category))
            {
                existing.Category = row.Category;
                touch = true;
            }

            if (!existing.IsActive)
            {
                // Re-activate known list SKUs so seed restores the published catalog set.
                existing.SetActive(true);
                touch = true;
            }

            if (touch)
                changed++;
        }

        if (changed > 0)
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        LogUpserted(logger, changed, null);
    }
}
