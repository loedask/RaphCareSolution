using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Identity;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.InviteClinicStaff;
public sealed class InviteClinicStaffHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IProfessionalUserLookupService professionalUserLookupService,
    IUserRoleAssignmentService roleAssignmentService,
    IClinicStaffInvitationService clinicStaffInvitationService,
    IClinicStaffPendingInvitationService clinicStaffPendingInvitationService,
    IRepository<Clinic> clinicRepository)
    : IRequestHandler<InviteClinicStaffCommand, ClinicStaffMemberDto?>
{
    public async Task<ClinicStaffMemberDto?> Handle(
        InviteClinicStaffCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUserService.CurrentUserId is not { } invitedByUserId)
            throw new ForbiddenAccessException("Only hospital administrators can invite staff.");

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
        {
            var pending = await clinicStaffPendingInvitationService
                .CreateAsync(request.ClinicId, request.Email, invitedByUserId, cancellationToken)
                .ConfigureAwait(false);

            return new ClinicStaffMemberDto
            {
                InvitationId = pending.InvitationId,
                IsPendingInvitation = true,
                Email = pending.Email,
                DisplayName = pending.Email,
                JoinedAt = pending.InvitedAt,
                IsActive = true,
                HasLoggedIn = false,
                LastInvitationSentAt = pending.LastInvitationSentAt
            };
        }

        if (!await professionalUserLookupService
                .IsProfessionalAsync(user.Id, cancellationToken)
                .ConfigureAwait(false))
            throw new BusinessRuleException(
                "That account is not a healthcare professional. They must use professional registration.");

        await roleAssignmentService
            .AssignRoleIfMissingAsync(user.Id, RaphCareRoles.Clinician, cancellationToken)
            .ConfigureAwait(false);

        await clinicStaffMembershipService
            .EnsureMembershipAsync(user.Id, request.ClinicId, cancellationToken)
            .ConfigureAwait(false);

        var clinic = await clinicRepository.GetByIdAsync(request.ClinicId, cancellationToken).ConfigureAwait(false);
        var hasLoggedIn = await StaffLoginStatus.HasLoggedInAsync(
            user.Id, clinic, professionalUserLookupService, cancellationToken).ConfigureAwait(false);

        if (!hasLoggedIn)
        {
            await clinicStaffInvitationService
                .SendInvitationAsync(request.ClinicId, user.Id, cancellationToken)
                .ConfigureAwait(false);
        }

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
            IsActive = membership?.IsActive ?? true,
            HasLoggedIn = hasLoggedIn,
            LastInvitationSentAt = membership?.LastInvitationSentAt
        };
    }
}
