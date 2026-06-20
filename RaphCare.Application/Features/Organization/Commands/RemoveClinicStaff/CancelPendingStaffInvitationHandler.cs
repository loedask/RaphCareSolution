using MediatR;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Organization.Commands.RemoveClinicStaff;

public sealed class CancelPendingStaffInvitationCommand : IRequest<bool>
{
    public Guid ClinicId { get; init; }
    public Guid InvitationId { get; init; }
}

public sealed class CancelPendingStaffInvitationHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IClinicStaffPendingInvitationService clinicStaffPendingInvitationService)
    : IRequestHandler<CancelPendingStaffInvitationCommand, bool>
{
    public async Task<bool> Handle(CancelPendingStaffInvitationCommand request, CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            return false;

        return await clinicStaffPendingInvitationService
            .CancelAsync(request.ClinicId, request.InvitationId, cancellationToken)
            .ConfigureAwait(false);
    }
}
