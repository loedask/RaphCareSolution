namespace RaphCare.Client.Models.Ops;

public sealed class PriceCatalogItem
{
    public Guid Id { get; set; }
    public string SkuCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal AmountZar { get; set; }
    public decimal AmountUsd { get; set; }
    public bool IsActive { get; set; }
}
