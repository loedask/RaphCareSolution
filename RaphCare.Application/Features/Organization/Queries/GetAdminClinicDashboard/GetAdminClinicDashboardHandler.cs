using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicDashboard;

public sealed class GetAdminClinicDashboardHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IAdminClinicDashboardQueryService adminClinicDashboardQueryService)
    : IRequestHandler<GetAdminClinicDashboardQuery, AdminClinicDashboardDto?>
{
    public async Task<AdminClinicDashboardDto?> Handle(
        GetAdminClinicDashboardQuery request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService, clinicStaffMembershipService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            return null;

        return await adminClinicDashboardQueryService
            .GetDashboardAsync(request.ClinicId, cancellationToken)
            .ConfigureAwait(false);
    }
}
