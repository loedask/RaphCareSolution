using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicCollectionOrders;

public sealed class GetAdminClinicCollectionOrdersHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IAdminClinicCollectionQueryService collectionQueryService)
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

        return await collectionQueryService
            .GetPendingOrdersAsync(request.ClinicId, request.Search, cancellationToken)
            .ConfigureAwait(false);
    }
}
