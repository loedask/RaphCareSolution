using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicAdmissionById;

public sealed class GetAdminClinicAdmissionByIdHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IAdminClinicInpatientQueryService inpatientQueryService)
    : IRequestHandler<GetAdminClinicAdmissionByIdQuery, AdminClinicAdmissionDto?>
{
    public async Task<AdminClinicAdmissionDto?> Handle(
        GetAdminClinicAdmissionByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService, clinicStaffMembershipService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            return null;

        return await inpatientQueryService
            .GetAdmissionByIdAsync(request.ClinicId, request.AdmissionId, cancellationToken)
            .ConfigureAwait(false);
    }
}
