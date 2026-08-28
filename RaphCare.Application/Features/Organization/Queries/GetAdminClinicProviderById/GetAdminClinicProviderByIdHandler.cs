using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicProviderById;

public sealed class GetAdminClinicProviderByIdHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IAdminClinicProviderQueryService adminClinicProviderQueryService)
    : IRequestHandler<GetAdminClinicProviderByIdQuery, AdminClinicProviderDetailDto?>
{
    public async Task<AdminClinicProviderDetailDto?> Handle(
        GetAdminClinicProviderByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService, clinicStaffMembershipService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            return null;

        return await adminClinicProviderQueryService
            .GetProviderDetailAsync(request.ClinicId, request.ProviderId, cancellationToken)
            .ConfigureAwait(false);
    }
}
