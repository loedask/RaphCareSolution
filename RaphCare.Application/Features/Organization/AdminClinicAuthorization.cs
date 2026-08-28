using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Identity;

namespace RaphCare.Application.Features.Organization;

internal static class AdminClinicAuthorization
{
    public static async Task<bool> HasClinicAccessAsync(
        ICurrentUserService currentUser,
        IClinicStaffMembershipService membershipService,
        Guid clinicId,
        CancellationToken cancellationToken)
    {
        if (currentUser.CurrentUserId is not { } userId)
            return false;

        return await membershipService
            .HasMembershipAsync(userId, clinicId, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<bool> IsClinicAdministratorAsync(
        ICurrentUserService currentUser,
        IClinicStaffMembershipService membershipService,
        IUserRoleAssignmentService roleAssignmentService,
        Guid clinicId,
        CancellationToken cancellationToken)
    {
        if (currentUser.CurrentUserId is not { } userId)
            return false;

        if (!await membershipService.HasMembershipAsync(userId, clinicId, cancellationToken).ConfigureAwait(false))
            return false;

        var roles = await roleAssignmentService.GetRoleNamesAsync(userId, cancellationToken).ConfigureAwait(false);
        return RaphCareRoles.HasAdministratorRole(roles);
    }
}
