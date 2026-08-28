using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicProviders;

public sealed class GetAdminClinicProvidersHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IAdminClinicProviderQueryService adminClinicProviderQueryService)
    : IRequestHandler<GetAdminClinicProvidersQuery, IReadOnlyList<AdminClinicProviderListItemDto>?>
{
    public async Task<IReadOnlyList<AdminClinicProviderListItemDto>?> Handle(
        GetAdminClinicProvidersQuery request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService, clinicStaffMembershipService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            return null;

        return await adminClinicProviderQueryService
            .GetProvidersAsync(request.ClinicId, cancellationToken)
            .ConfigureAwait(false);
    }
}
