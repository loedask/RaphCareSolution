using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicStaff;

public sealed class GetAdminClinicStaffHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IProfessionalUserLookupService professionalUserLookupService,
    IUserRoleAssignmentService roleAssignmentService)
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

        var memberships = await clinicStaffMembershipService
            .GetStaffMembershipsAsync(request.ClinicId, cancellationToken)
            .ConfigureAwait(false);

        if (memberships.Count == 0)
            return Array.Empty<ClinicStaffMemberDto>();

        var userIds = memberships.Select(m => m.ApplicationUserId).ToList();
        var users = await professionalUserLookupService
            .GetUsersByIdsAsync(userIds, cancellationToken)
            .ConfigureAwait(false);
        var usersById = users.ToDictionary(u => u.Id);

        var result = new List<ClinicStaffMemberDto>();
        foreach (var membership in memberships.Where(m => m.IsActive).OrderBy(m => m.JoinedAt))
        {
            if (!usersById.TryGetValue(membership.ApplicationUserId, out var user))
                continue;

            var roles = await roleAssignmentService
                .GetRoleNamesAsync(user.Id, cancellationToken)
                .ConfigureAwait(false);

            result.Add(new ClinicStaffMemberDto
            {
                UserId = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                Roles = roles,
                JoinedAt = membership.JoinedAt,
                IsActive = membership.IsActive
            });
        }

        return result;
    }
}
