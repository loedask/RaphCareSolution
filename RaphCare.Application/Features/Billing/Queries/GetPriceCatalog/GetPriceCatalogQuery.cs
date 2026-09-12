using MediatR;
using RaphCare.Application.Features.Billing.DTOs;

namespace RaphCare.Application.Features.Billing.Queries.GetPriceCatalog;

public sealed class GetPriceCatalogQuery : IRequest<IReadOnlyList<PriceCatalogItemDto>>
{
    public bool ActiveOnly { get; init; } = true;
}
