using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Identity;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization;

internal static class AdminClinicStaffRules
{
    public static async Task<bool> HasAnotherClinicAdministratorAsync(
        Guid clinicId,
        Guid excludingUserId,
        IClinicStaffMembershipService clinicStaffMembershipService,
        IUserRoleAssignmentService roleAssignmentService,
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
            if (RaphCareRoles.HasAdministratorRole(roles))
                return true;
        }

        return false;
    }

    public static async Task ValidateStaffRemovalAsync(
        Clinic clinic,
        Guid userIdToRemove,
        Guid? currentUserId,
        IClinicStaffMembershipService clinicStaffMembershipService,
        IUserRoleAssignmentService roleAssignmentService,
        CancellationToken cancellationToken)
    {
        if (currentUserId == userIdToRemove)
            throw new Common.Exceptions.BusinessRuleException("You cannot remove yourself from the hospital.");

        if (clinic.RegisteredByApplicationUserId == userIdToRemove)
            throw new Common.Exceptions.BusinessRuleException("The hospital registrant cannot be removed.");

        var roles = await roleAssignmentService
            .GetRoleNamesAsync(userIdToRemove, cancellationToken)
            .ConfigureAwait(false);

        if (RaphCareRoles.HasAdministratorRole(roles)
            && !await HasAnotherClinicAdministratorAsync(
                clinic.Id,
                userIdToRemove,
                clinicStaffMembershipService,
                roleAssignmentService,
                cancellationToken)
                .ConfigureAwait(false))
        {
            throw new Common.Exceptions.BusinessRuleException(
                "At least one other administrator must remain for this hospital.");
        }
    }
}
