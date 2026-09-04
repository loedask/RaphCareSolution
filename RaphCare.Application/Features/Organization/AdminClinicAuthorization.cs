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

    /// <summary>
    /// Membership access, or platform Administrator (Ops fleet and cross-hospital admin reads).
    /// </summary>
    public static async Task<bool> HasClinicAccessOrPlatformAdminAsync(
        ICurrentUserService currentUser,
        IClinicStaffMembershipService membershipService,
        IUserRoleAssignmentService roleAssignmentService,
        Guid clinicId,
        CancellationToken cancellationToken)
    {
        if (await HasClinicAccessAsync(currentUser, membershipService, clinicId, cancellationToken)
            .ConfigureAwait(false))
            return true;

        return await IsPlatformAdministratorAsync(currentUser, roleAssignmentService, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<bool> IsPlatformAdministratorAsync(
        ICurrentUserService currentUser,
        IUserRoleAssignmentService roleAssignmentService,
        CancellationToken cancellationToken)
    {
        if (currentUser.CurrentUserId is not { } userId)
            return false;

        var roles = await roleAssignmentService
            .GetRoleNamesAsync(userId, cancellationToken)
            .ConfigureAwait(false);
        return RaphCareRoles.HasAdministratorRole(roles);
    }

    public static async Task<IReadOnlyList<string>> GetClinicStaffRolesAsync(
        ICurrentUserService currentUser,
        IClinicStaffMembershipService membershipService,
        IUserRoleAssignmentService roleAssignmentService,
        Guid clinicId,
        CancellationToken cancellationToken)
    {
        if (currentUser.CurrentUserId is not { } userId)
            return [];

        if (!await membershipService.HasMembershipAsync(userId, clinicId, cancellationToken).ConfigureAwait(false))
            return [];

        return await roleAssignmentService.GetRoleNamesAsync(userId, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<bool> IsClinicAdministratorAsync(
        ICurrentUserService currentUser,
        IClinicStaffMembershipService membershipService,
        IUserRoleAssignmentService roleAssignmentService,
        Guid clinicId,
        CancellationToken cancellationToken)
    {
        var roles = await GetClinicStaffRolesAsync(
                currentUser, membershipService, roleAssignmentService, clinicId, cancellationToken)
            .ConfigureAwait(false);
        return RaphCareRoles.HasAdministratorRole(roles);
    }

    public static async Task<bool> CanDocumentVisitsAsync(
        ICurrentUserService currentUser,
        IClinicStaffMembershipService membershipService,
        IUserRoleAssignmentService roleAssignmentService,
        Guid clinicId,
        CancellationToken cancellationToken)
    {
        var roles = await GetClinicStaffRolesAsync(
                currentUser, membershipService, roleAssignmentService, clinicId, cancellationToken)
            .ConfigureAwait(false);
        return RaphCareRoles.CanDocumentVisits(roles);
    }

    public static async Task<bool> CanRecordWardNotesAsync(
        ICurrentUserService currentUser,
        IClinicStaffMembershipService membershipService,
        IUserRoleAssignmentService roleAssignmentService,
        Guid clinicId,
        CancellationToken cancellationToken)
    {
        var roles = await GetClinicStaffRolesAsync(
                currentUser, membershipService, roleAssignmentService, clinicId, cancellationToken)
            .ConfigureAwait(false);
        return RaphCareRoles.CanRecordWardNotes(roles);
    }

    public static async Task EnsureClinicStaffAsync(
        ICurrentUserService currentUser,
        IClinicStaffMembershipService membershipService,
        Guid clinicId,
        string forbiddenMessage,
        CancellationToken cancellationToken)
    {
        if (!await HasClinicAccessAsync(currentUser, membershipService, clinicId, cancellationToken).ConfigureAwait(false))
            throw new ForbiddenAccessException(forbiddenMessage);
    }

    public static async Task EnsureCanDocumentVisitsAsync(
        ICurrentUserService currentUser,
        IClinicStaffMembershipService membershipService,
        IUserRoleAssignmentService roleAssignmentService,
        Guid clinicId,
        CancellationToken cancellationToken)
    {
        if (!await CanDocumentVisitsAsync(
                currentUser, membershipService, roleAssignmentService, clinicId, cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only a doctor or hospital administrator can document this visit.");
    }

    public static async Task EnsureCanDispenseAsync(
        ICurrentUserService currentUser,
        IClinicStaffMembershipService membershipService,
        IUserRoleAssignmentService roleAssignmentService,
        Guid clinicId,
        CancellationToken cancellationToken)
    {
        var roles = await GetClinicStaffRolesAsync(
                currentUser, membershipService, roleAssignmentService, clinicId, cancellationToken)
            .ConfigureAwait(false);
        if (!RaphCareRoles.CanDispensePrescriptions(roles))
            throw new ForbiddenAccessException("Only pharmacy staff can mark a prescription as collected.");
    }

    public static async Task EnsureCanCompleteLabsAsync(
        ICurrentUserService currentUser,
        IClinicStaffMembershipService membershipService,
        IUserRoleAssignmentService roleAssignmentService,
        Guid clinicId,
        CancellationToken cancellationToken)
    {
        var roles = await GetClinicStaffRolesAsync(
                currentUser, membershipService, roleAssignmentService, clinicId, cancellationToken)
            .ConfigureAwait(false);
        if (!RaphCareRoles.CanCompleteLabs(roles))
            throw new ForbiddenAccessException("Only lab staff can enter this result.");
    }
}
