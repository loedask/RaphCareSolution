using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicPatientById;

public sealed class GetAdminClinicPatientByIdHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IAdminClinicPatientQueryService adminClinicPatientQueryService)
    : IRequestHandler<GetAdminClinicPatientByIdQuery, AdminClinicPatientDetailDto?>
{
    public async Task<AdminClinicPatientDetailDto?> Handle(
        GetAdminClinicPatientByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService, clinicStaffMembershipService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            return null;

        return await adminClinicPatientQueryService
            .GetPatientDetailAsync(request.ClinicId, request.PatientId, cancellationToken)
            .ConfigureAwait(false);
    }
}
