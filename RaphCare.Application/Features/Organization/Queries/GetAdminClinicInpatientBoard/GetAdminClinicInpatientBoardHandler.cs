using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicInpatientBoard;

public sealed class GetAdminClinicInpatientBoardHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IAdminClinicInpatientQueryService inpatientQueryService)
    : IRequestHandler<GetAdminClinicInpatientBoardQuery, AdminClinicInpatientBoardDto?>
{
    public async Task<AdminClinicInpatientBoardDto?> Handle(
        GetAdminClinicInpatientBoardQuery request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService, clinicStaffMembershipService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            return null;

        return await inpatientQueryService.GetBoardAsync(request.ClinicId, cancellationToken).ConfigureAwait(false);
    }
}
