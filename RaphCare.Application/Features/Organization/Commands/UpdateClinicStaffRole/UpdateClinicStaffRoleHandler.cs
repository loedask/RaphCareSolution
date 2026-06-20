using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Organization.Commands.UpdateClinicStaffRole;

public sealed class UpdateClinicStaffRoleHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService)
    : IRequestHandler<UpdateClinicStaffRoleCommand, bool>
{
    public async Task<bool> Handle(UpdateClinicStaffRoleCommand request, CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can change staff roles.");

        if (currentUserService.CurrentUserId == request.UserId)
            throw new BusinessRuleException("You cannot change your own administrator role.");

        if (!await clinicStaffMembershipService
                .HasMembershipAsync(request.UserId, request.ClinicId, cancellationToken)
                .ConfigureAwait(false))
            return false;

        var roles = await roleAssignmentService
            .GetRoleNamesAsync(request.UserId, cancellationToken)
            .ConfigureAwait(false);
        var isCurrentlyAdministrator = roles.Contains("Administrator", StringComparer.OrdinalIgnoreCase);

        if (request.IsAdministrator)
        {
            if (isCurrentlyAdministrator)
                return true;

            await roleAssignmentService
                .AssignRoleIfMissingAsync(request.UserId, "Clinician", cancellationToken)
                .ConfigureAwait(false);
            await roleAssignmentService
                .AssignRoleIfMissingAsync(request.UserId, "Administrator", cancellationToken)
                .ConfigureAwait(false);
            return true;
        }

        if (!isCurrentlyAdministrator)
            return true;

        if (!await HasAnotherClinicAdministratorAsync(request.ClinicId, request.UserId, cancellationToken)
                .ConfigureAwait(false))
            throw new BusinessRuleException("At least one other administrator must remain for this hospital.");

        await roleAssignmentService
            .RemoveRoleAsync(request.UserId, "Administrator", cancellationToken)
            .ConfigureAwait(false);
        await roleAssignmentService
            .AssignRoleIfMissingAsync(request.UserId, "Clinician", cancellationToken)
            .ConfigureAwait(false);
        return true;
    }

    private async Task<bool> HasAnotherClinicAdministratorAsync(
        Guid clinicId,
        Guid excludingUserId,
        CancellationToken cancellationToken)
    {
        var memberships = await clinicStaffMembershipService
            .GetStaffMembershipsAsync(clinicId, cancellationToken)
            .ConfigureAwait(false);

        foreach (var membership in memberships.Where(m => m.IsActive && m.ApplicationUserId != excludingUserId))
        {
            var roles = await roleAssignmentService
                .GetRoleNamesAsync(membership.ApplicationUserId, cancellationToken)
                .ConfigureAwait(false);
            if (roles.Contains("Administrator", StringComparer.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
