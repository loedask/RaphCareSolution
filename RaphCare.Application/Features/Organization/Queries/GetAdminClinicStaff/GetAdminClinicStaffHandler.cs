using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicStaff;

public sealed class GetAdminClinicStaffHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IProfessionalUserLookupService professionalUserLookupService,
    IUserRoleAssignmentService roleAssignmentService,
    IClinicStaffPendingInvitationService clinicStaffPendingInvitationService,
    IRepository<Clinic> clinicRepository)
    : IRequestHandler<GetAdminClinicStaffQuery, IReadOnlyList<ClinicStaffMemberDto>?>
{
    public async Task<IReadOnlyList<ClinicStaffMemberDto>?> Handle(
        GetAdminClinicStaffQuery request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService, clinicStaffMembershipService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            return null;

        var clinic = await clinicRepository.GetByIdAsync(request.ClinicId, cancellationToken).ConfigureAwait(false);

        var memberships = await clinicStaffMembershipService
            .GetStaffMembershipsAsync(request.ClinicId, cancellationToken)
            .ConfigureAwait(false);

        var pendingInvitations = await clinicStaffPendingInvitationService
            .GetPendingForClinicAsync(request.ClinicId, cancellationToken)
            .ConfigureAwait(false);

        var result = new List<ClinicStaffMemberDto>();

        if (memberships.Count > 0)
        {
            var userIds = memberships.Select(m => m.ApplicationUserId).ToList();
            var users = await professionalUserLookupService
                .GetUsersByIdsAsync(userIds, cancellationToken)
                .ConfigureAwait(false);
            var usersById = users.ToDictionary(u => u.Id);

            foreach (var membership in memberships.Where(m => m.IsActive).OrderBy(m => m.JoinedAt))
            {
                if (!usersById.TryGetValue(membership.ApplicationUserId, out var user))
                    continue;

                var roles = await roleAssignmentService
                    .GetRoleNamesAsync(user.Id, cancellationToken)
                    .ConfigureAwait(false);

                var hasLoggedIn = await StaffLoginStatus.HasLoggedInAsync(
                    user.Id, clinic, professionalUserLookupService, cancellationToken).ConfigureAwait(false);

                result.Add(new ClinicStaffMemberDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    DisplayName = user.DisplayName,
                    Roles = roles,
                    JoinedAt = membership.JoinedAt,
                    IsActive = membership.IsActive,
                    HasLoggedIn = hasLoggedIn,
                    LastInvitationSentAt = membership.LastInvitationSentAt
                });
            }
        }

        foreach (var pending in pendingInvitations)
        {
            result.Add(new ClinicStaffMemberDto
            {
                InvitationId = pending.InvitationId,
                IsPendingInvitation = true,
                Email = pending.Email,
                DisplayName = pending.Email,
                Roles = string.IsNullOrWhiteSpace(pending.JobRole)
                    ? Array.Empty<string>()
                    : [pending.JobRole],
                JoinedAt = pending.InvitedAt,
                IsActive = true,
                HasLoggedIn = false,
                LastInvitationSentAt = pending.LastInvitationSentAt
            });
        }

        return result;
    }
}
