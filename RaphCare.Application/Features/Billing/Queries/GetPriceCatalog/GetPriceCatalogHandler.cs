using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Billing.DTOs;
using RaphCare.Application.Features.Organization;
using RaphCare.Domain.Billing;

namespace RaphCare.Application.Features.Billing.Queries.GetPriceCatalog;

public sealed class GetPriceCatalogHandler(
    IRepository<PriceCatalogItem> catalogRepository,
    ICurrentUserService currentUserService,
    IUserRoleAssignmentService roleAssignmentService)
    : IRequestHandler<GetPriceCatalogQuery, IReadOnlyList<PriceCatalogItemDto>>
{
    public async Task<IReadOnlyList<PriceCatalogItemDto>> Handle(
        GetPriceCatalogQuery request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsPlatformAdministratorAsync(
                currentUserService, roleAssignmentService, cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only platform Ops can view the price catalog.");

        var page = await catalogRepository.SearchAsync(
            queryShaper: q =>
            {
                if (request.ActiveOnly)
                    q = q.Where(i => i.IsActive);
                return q.OrderBy(i => i.Category).ThenBy(i => i.SkuCode);
            },
            pageNumber: 1,
            pageSize: 500,
            applyDefaultIdOrdering: false,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return page.Items
            .Select(i => new PriceCatalogItemDto
            {
                Id = i.Id,
                SkuCode = i.SkuCode,
                DisplayName = i.DisplayName,
                Category = i.Category,
                AmountZar = i.AmountZar,
                AmountUsd = i.AmountUsd,
                IsActive = i.IsActive
            })
            .ToList();
    }
}
