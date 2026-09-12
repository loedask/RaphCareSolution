using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicCollectionOrders;

public sealed class GetAdminClinicCollectionOrdersHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IAdminClinicCollectionQueryService collectionQueryService,
    IRepository<Clinic> clinicRepository)
    : IRequestHandler<GetAdminClinicCollectionOrdersQuery, AdminClinicCollectionBoardDto?>
{
    public async Task<AdminClinicCollectionBoardDto?> Handle(
        GetAdminClinicCollectionOrdersQuery request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService, clinicStaffMembershipService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            return null;

        await AdminClinicCommercialPlanGuard.EnsureFeatureAsync(
                clinicRepository,
                request.ClinicId,
                ClinicCommercialPlanFeatures.HasCollection,
                "Collection",
                cancellationToken)
            .ConfigureAwait(false);

        return await collectionQueryService
            .GetPendingOrdersAsync(request.ClinicId, request.Search, cancellationToken)
            .ConfigureAwait(false);
    }
}
