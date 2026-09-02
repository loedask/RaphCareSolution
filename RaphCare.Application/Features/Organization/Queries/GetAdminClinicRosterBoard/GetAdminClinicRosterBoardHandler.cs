using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicRosterBoard;

public sealed class GetAdminClinicRosterBoardHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IProfessionalUserLookupService professionalUserLookupService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Clinic> clinicRepository,
    IRepository<ClinicRosterEntry> rosterRepository,
    IDateTimeProvider clock)
    : IRequestHandler<GetAdminClinicRosterBoardQuery, AdminClinicRosterBoardDto?>
{
    public async Task<AdminClinicRosterBoardDto?> Handle(
        GetAdminClinicRosterBoardQuery request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService, clinicStaffMembershipService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            return null;

        var clinic = await clinicRepository.GetByIdAsync(request.ClinicId, cancellationToken).ConfigureAwait(false);
        if (clinic is null)
            return null;

        var day = ClinicRosterShift.NormalizeDutyDate(request.DayUtc ?? clock.UtcNow);
        var page = await rosterRepository.SearchAsync(
            q => q.Where(e => e.ClinicId == request.ClinicId && e.DutyDate == day),
            1,
            500,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var entries = page.Items
            .OrderBy(e => ClinicRosterShift.SortOrder(e.ShiftLabel))
            .ThenBy(e => e.CreatedAt)
            .ToList();

        var userIds = entries.Select(e => e.ApplicationUserId).Distinct().ToList();
        var users = userIds.Count == 0
            ? []
            : await professionalUserLookupService.GetUsersByIdsAsync(userIds, cancellationToken).ConfigureAwait(false);
        var usersById = users.ToDictionary(u => u.Id);

        var dtoEntries = new List<AdminClinicRosterEntryDto>(entries.Count);
        foreach (var entry in entries)
        {
            usersById.TryGetValue(entry.ApplicationUserId, out var user);
            var roles = user is null
                ? Array.Empty<string>()
                : await roleAssignmentService.GetRoleNamesAsync(user.Id, cancellationToken).ConfigureAwait(false);

            dtoEntries.Add(new AdminClinicRosterEntryDto
            {
                Id = entry.Id,
                ApplicationUserId = entry.ApplicationUserId,
                DisplayName = user is null
                    ? "Staff"
                    : string.IsNullOrWhiteSpace(user.DisplayName) ? (user.Email ?? "Staff") : user.DisplayName,
                Email = user?.Email,
                Roles = roles,
                DutyDate = entry.DutyDate,
                ShiftLabel = entry.ShiftLabel,
                Note = entry.Note
            });
        }

        return new AdminClinicRosterBoardDto
        {
            ClinicName = clinic.Name,
            DayUtc = day,
            MorningCount = dtoEntries.Count(e => e.ShiftLabel == ClinicRosterShift.Morning),
            AfternoonCount = dtoEntries.Count(e => e.ShiftLabel == ClinicRosterShift.Afternoon),
            NightCount = dtoEntries.Count(e => e.ShiftLabel == ClinicRosterShift.Night),
            Entries = dtoEntries
        };
    }
}
