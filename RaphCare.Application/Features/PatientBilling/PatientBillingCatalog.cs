using RaphCare.Application.Features.PatientBilling.DTOs;
using RaphCare.Domain.Billing;

namespace RaphCare.Application.Features.PatientBilling;

/// <summary>Published care / app plan tiers. Prefers DB price catalog amounts when present.</summary>
public static class PatientBillingCatalog
{
    public const string FreeCode = "FREE";
    public const string EssentialCode = "ESSENTIAL";
    public const string CompleteCode = "COMPLETE";

    public static IReadOnlyList<PatientBillingPlanOptionDto> FallbackAll { get; } =
    [
        new()
        {
            PlanCode = FreeCode,
            DisplayName = "Free",
            Tier = 0,
            MonthlyPrice = 0,
            Currency = "ZAR",
            Description = "Core app access and health record viewing."
        },
        new()
        {
            PlanCode = EssentialCode,
            DisplayName = "Essential Care",
            Tier = 1,
            MonthlyPrice = 149,
            Currency = "ZAR",
            Description = "Priority messaging, extended device history, and care reminders."
        },
        new()
        {
            PlanCode = CompleteCode,
            DisplayName = "Complete Care",
            Tier = 2,
            MonthlyPrice = 299,
            Currency = "ZAR",
            Description = "Everything in Essential plus telehealth credits and family sharing (when available)."
        }
    ];

    /// <summary>Fallback list used when the catalog is empty or missing care SKUs.</summary>
    public static IReadOnlyList<PatientBillingPlanOptionDto> All => FallbackAll;

    public static PatientBillingPlanOptionDto? FindByCode(string? planCode) =>
        FindByCode(planCode, FallbackAll);

    public static PatientBillingPlanOptionDto? FindByCode(
        string? planCode,
        IReadOnlyList<PatientBillingPlanOptionDto> options)
    {
        if (string.IsNullOrWhiteSpace(planCode))
            return null;
        var c = planCode.Trim().ToUpperInvariant();
        return options.FirstOrDefault(p => string.Equals(p.PlanCode, c, StringComparison.Ordinal));
    }

    /// <summary>
    /// Builds patient care plan options from catalog items. Falls back to constants when care SKUs are missing.
    /// </summary>
    public static IReadOnlyList<PatientBillingPlanOptionDto> FromCatalog(
        IEnumerable<PriceCatalogItem>? catalogItems)
    {
        if (catalogItems is null)
            return FallbackAll;

        var bySku = catalogItems
            .Where(i => i.IsActive)
            .ToDictionary(i => i.SkuCode, StringComparer.OrdinalIgnoreCase);

        if (!bySku.ContainsKey(PriceCatalogSku.CareEssential)
            && !bySku.ContainsKey(PriceCatalogSku.CareComplete))
            return FallbackAll;

        var free = FallbackAll[0];
        var essentialFallback = FallbackAll[1];
        var completeFallback = FallbackAll[2];

        bySku.TryGetValue(PriceCatalogSku.CareEssential, out var essentialItem);
        bySku.TryGetValue(PriceCatalogSku.CareComplete, out var completeItem);

        return
        [
            free,
            new PatientBillingPlanOptionDto
            {
                PlanCode = EssentialCode,
                DisplayName = essentialItem?.DisplayName ?? essentialFallback.DisplayName,
                Tier = 1,
                MonthlyPrice = essentialItem?.AmountZar ?? essentialFallback.MonthlyPrice,
                Currency = "ZAR",
                Description = essentialFallback.Description
            },
            new PatientBillingPlanOptionDto
            {
                PlanCode = CompleteCode,
                DisplayName = completeItem?.DisplayName ?? completeFallback.DisplayName,
                Tier = 2,
                MonthlyPrice = completeItem?.AmountZar ?? completeFallback.MonthlyPrice,
                Currency = "ZAR",
                Description = completeFallback.Description
            }
        ];
    }
}
