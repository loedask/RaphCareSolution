using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.InviteClinicStaff;
public sealed class InviteClinicStaffHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IProfessionalUserLookupService professionalUserLookupService,
    IUserRoleAssignmentService roleAssignmentService)
    : IRequestHandler<InviteClinicStaffCommand, ClinicStaffMemberDto?>
{
    public async Task<ClinicStaffMemberDto?> Handle(
        InviteClinicStaffCommand request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can invite staff.");

        var user = await professionalUserLookupService
            .FindByEmailAsync(request.Email, cancellationToken)
            .ConfigureAwait(false);
        if (user is null)
            throw new BusinessRuleException(
                "No account found with that email. Ask them to register as a healthcare professional first.");

        if (!await professionalUserLookupService
                .IsProfessionalAsync(user.Id, cancellationToken)
                .ConfigureAwait(false))
            throw new BusinessRuleException(
                "That account is not a healthcare professional. They must use professional registration.");

        await roleAssignmentService
            .AssignRoleIfMissingAsync(user.Id, "Clinician", cancellationToken)
            .ConfigureAwait(false);

        await clinicStaffMembershipService
            .EnsureMembershipAsync(user.Id, request.ClinicId, cancellationToken)
            .ConfigureAwait(false);

        var roles = await roleAssignmentService
            .GetRoleNamesAsync(user.Id, cancellationToken)
            .ConfigureAwait(false);

        var memberships = await clinicStaffMembershipService
            .GetStaffMembershipsAsync(request.ClinicId, cancellationToken)
            .ConfigureAwait(false);
        var membership = memberships.FirstOrDefault(m => m.ApplicationUserId == user.Id);

        return new ClinicStaffMemberDto
        {
            UserId = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            Roles = roles,
            JoinedAt = membership?.JoinedAt ?? DateTime.UtcNow,
            IsActive = membership?.IsActive ?? true
        };
    }
}
