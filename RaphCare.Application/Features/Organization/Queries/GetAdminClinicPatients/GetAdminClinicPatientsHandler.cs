using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicPatients;

public sealed class GetAdminClinicPatientsHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IAdminClinicPatientQueryService adminClinicPatientQueryService)
    : IRequestHandler<GetAdminClinicPatientsQuery, PagedResult<AdminClinicPatientListItemDto>?>
{
    public async Task<PagedResult<AdminClinicPatientListItemDto>?> Handle(
        GetAdminClinicPatientsQuery request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService, clinicStaffMembershipService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            return null;

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);

        return await adminClinicPatientQueryService
            .GetPatientsAsync(request.ClinicId, pageNumber, pageSize, request.Search, cancellationToken)
            .ConfigureAwait(false);
    }
}
