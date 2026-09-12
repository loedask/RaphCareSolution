using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Billing.DTOs;
using RaphCare.Application.Features.Organization;
using RaphCare.Domain.Billing;

namespace RaphCare.Application.Features.Billing.Commands.UpdatePriceCatalogItem;

public sealed class UpdatePriceCatalogItemHandler(
    IRepository<PriceCatalogItem> catalogRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IUserRoleAssignmentService roleAssignmentService)
    : IRequestHandler<UpdatePriceCatalogItemCommand, PriceCatalogItemDto?>
{
    public async Task<PriceCatalogItemDto?> Handle(
        UpdatePriceCatalogItemCommand request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsPlatformAdministratorAsync(
                currentUserService, roleAssignmentService, cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only platform Ops can update list prices.");

        var item = await catalogRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (item is null)
            return null;

        item.SetAmounts(request.AmountZar, request.AmountUsd);
        await catalogRepository.UpdateAsync(item, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new PriceCatalogItemDto
        {
            Id = item.Id,
            SkuCode = item.SkuCode,
            DisplayName = item.DisplayName,
            Category = item.Category,
            AmountZar = item.AmountZar,
            AmountUsd = item.AmountUsd,
            IsActive = item.IsActive
        };
    }
}
