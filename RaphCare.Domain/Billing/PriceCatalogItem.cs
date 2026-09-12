using RaphCare.Domain.Common;

namespace RaphCare.Domain.Billing;

/// <summary>Platform list price for a commercial SKU (site plan, seat, care tier, package, usage, or add-on).</summary>
public class PriceCatalogItem : BaseEntity
{
    /// <summary>Stable product code, e.g. SITE_CLINIC or CARE_ESSENTIAL.</summary>
    public string SkuCode { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Site, Seat, PatientCare, WatchPackage, Usage, or Addon.</summary>
    public string Category { get; set; } = string.Empty;

    public decimal AmountZar { get; set; }

    public decimal AmountUsd { get; set; }

    public bool IsActive { get; set; } = true;

    public void SetAmounts(decimal amountZar, decimal amountUsd)
    {
        AmountZar = amountZar;
        AmountUsd = amountUsd;
        SetUpdated();
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        SetUpdated();
    }
}
