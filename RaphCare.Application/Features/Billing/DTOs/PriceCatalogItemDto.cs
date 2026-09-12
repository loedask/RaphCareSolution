namespace RaphCare.Application.Features.Billing.DTOs;

public sealed class PriceCatalogItemDto
{
    public Guid Id { get; init; }
    public string SkuCode { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public decimal AmountZar { get; init; }
    public decimal AmountUsd { get; init; }
    public bool IsActive { get; init; }
}
