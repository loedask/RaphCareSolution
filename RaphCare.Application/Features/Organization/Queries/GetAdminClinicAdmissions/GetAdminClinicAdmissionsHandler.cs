using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicAdmissions;

public sealed class GetAdminClinicAdmissionsHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IAdminClinicInpatientQueryService inpatientQueryService)
    : IRequestHandler<GetAdminClinicAdmissionsQuery, PagedResult<AdminClinicAdmissionDto>?>
{
    public async Task<PagedResult<AdminClinicAdmissionDto>?> Handle(
        GetAdminClinicAdmissionsQuery request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService, clinicStaffMembershipService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            return null;

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        return await inpatientQueryService
            .GetAdmissionsAsync(request.ClinicId, pageNumber, pageSize, request.Status, cancellationToken)
            .ConfigureAwait(false);
    }
}
