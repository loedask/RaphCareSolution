using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Organization.Commands.RemoveClinicStaff;

public sealed class RemoveClinicStaffHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService)
    : IRequestHandler<RemoveClinicStaffCommand, bool>
{
    public async Task<bool> Handle(RemoveClinicStaffCommand request, CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            return false;

        if (currentUserService.CurrentUserId == request.UserId)
            throw new BusinessRuleException("You cannot remove yourself from the hospital.");

        return await clinicStaffMembershipService
            .DeactivateMembershipAsync(request.UserId, request.ClinicId, cancellationToken)
            .ConfigureAwait(false);
    }
}
