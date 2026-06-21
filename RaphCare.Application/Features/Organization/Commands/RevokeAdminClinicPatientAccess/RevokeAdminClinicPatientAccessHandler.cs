using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Organization.Commands.RevokeAdminClinicPatientAccess;

public sealed class RevokeAdminClinicPatientAccessHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IPatientClinicAccessService patientClinicAccessService)
    : IRequestHandler<RevokeAdminClinicPatientAccessCommand, bool>
{
    public async Task<bool> Handle(RevokeAdminClinicPatientAccessCommand request, CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can revoke patient access.");

        if (!await patientClinicAccessService
                .HasClinicAccessAsync(request.PatientId, request.ClinicId, cancellationToken)
                .ConfigureAwait(false))
            return false;

        await patientClinicAccessService
            .RevokeClinicAccessAsync(request.PatientId, request.ClinicId, cancellationToken)
            .ConfigureAwait(false);
        return true;
    }
}
