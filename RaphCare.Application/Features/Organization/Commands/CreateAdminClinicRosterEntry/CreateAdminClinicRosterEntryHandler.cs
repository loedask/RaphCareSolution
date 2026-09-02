using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicRosterEntry;

public sealed class CreateAdminClinicRosterEntryHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IProfessionalUserLookupService professionalUserLookupService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<ClinicRosterEntry> rosterRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAdminClinicRosterEntryCommand, AdminClinicRosterEntryDto?>
{
    public async Task<AdminClinicRosterEntryDto?> Handle(
        CreateAdminClinicRosterEntryCommand request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can update the roster.",
                cancellationToken)
            .ConfigureAwait(false);

        var memberships = await clinicStaffMembershipService
            .GetStaffMembershipsAsync(request.ClinicId, cancellationToken)
            .ConfigureAwait(false);
        if (!memberships.Any(m => m.IsActive && m.ApplicationUserId == request.ApplicationUserId))
            throw new BusinessRuleException("That person is not active staff at this hospital.");

        var dutyDate = ClinicRosterShift.NormalizeDutyDate(request.DutyDate);
        var shift = ClinicRosterShift.NormalizeLabel(request.ShiftLabel);
        if (!ClinicRosterShift.Allowed.Contains(shift))
            throw new BusinessRuleException("Shift must be Morning, Afternoon, or Night.");

        var existing = await rosterRepository.SearchAsync(
            q => q.Where(e =>
                e.ClinicId == request.ClinicId
                && e.DutyDate == dutyDate
                && e.ApplicationUserId == request.ApplicationUserId
                && e.ShiftLabel == shift),
            1,
            1,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);
        if (existing.TotalCount > 0)
            throw new BusinessRuleException("That person is already on this shift for that day.");

        var entry = new ClinicRosterEntry
        {
            ClinicId = request.ClinicId,
            ApplicationUserId = request.ApplicationUserId,
            DutyDate = dutyDate,
            ShiftLabel = shift,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            CreatedByApplicationUserId = currentUserService.CurrentUserId
        };

        await rosterRepository.AddAsync(entry, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var users = await professionalUserLookupService
            .GetUsersByIdsAsync([request.ApplicationUserId], cancellationToken)
            .ConfigureAwait(false);
        var user = users.Count > 0 ? users[0] : null;
        var roles = user is null
            ? Array.Empty<string>()
            : await roleAssignmentService.GetRoleNamesAsync(user.Id, cancellationToken).ConfigureAwait(false);

        return new AdminClinicRosterEntryDto
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
        };
    }
}
