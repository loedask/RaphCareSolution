using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Organization.Commands.ResendClinicStaffInvitation;

public sealed class ResendPendingStaffInvitationCommand : IRequest<bool>
{
    public Guid ClinicId { get; init; }
    public Guid InvitationId { get; init; }
}

public sealed class ResendPendingStaffInvitationHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IClinicStaffPendingInvitationService clinicStaffPendingInvitationService)
    : IRequestHandler<ResendPendingStaffInvitationCommand, bool>
{
    public async Task<bool> Handle(ResendPendingStaffInvitationCommand request, CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can resend invitations.");

        return await clinicStaffPendingInvitationService
            .ResendAsync(request.ClinicId, request.InvitationId, cancellationToken)
            .ConfigureAwait(false);
    }
}
