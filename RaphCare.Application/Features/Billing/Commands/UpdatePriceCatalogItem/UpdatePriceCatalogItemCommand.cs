using MediatR;
using RaphCare.Application.Features.Billing.DTOs;

namespace RaphCare.Application.Features.Billing.Commands.UpdatePriceCatalogItem;

public sealed class UpdatePriceCatalogItemCommand : IRequest<PriceCatalogItemDto?>
{
    public Guid Id { get; init; }
    public decimal AmountZar { get; init; }
    public decimal AmountUsd { get; init; }
}
