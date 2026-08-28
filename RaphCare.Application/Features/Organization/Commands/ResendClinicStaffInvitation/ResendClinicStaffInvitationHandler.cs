using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.ResendClinicStaffInvitation;

public sealed class ResendClinicStaffInvitationHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Clinic> clinicRepository,
    IProfessionalUserLookupService professionalUserLookupService,
    IClinicStaffInvitationService clinicStaffInvitationService)
    : IRequestHandler<ResendClinicStaffInvitationCommand, bool>
{
    public async Task<bool> Handle(ResendClinicStaffInvitationCommand request, CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can resend invitations.");

        if (!await clinicStaffMembershipService
                .HasMembershipAsync(request.UserId, request.ClinicId, cancellationToken)
                .ConfigureAwait(false))
            return false;

        var clinic = await clinicRepository.GetByIdAsync(request.ClinicId, cancellationToken).ConfigureAwait(false);
        if (clinic is null)
            return false;

        if (await StaffLoginStatus.HasLoggedInAsync(
                request.UserId, clinic, professionalUserLookupService, cancellationToken)
            .ConfigureAwait(false))
            throw new BusinessRuleException("This staff member has already signed in. Resend is only for pending invitations.");

        await clinicStaffInvitationService
            .SendInvitationAsync(request.ClinicId, request.UserId, cancellationToken)
            .ConfigureAwait(false);

        return true;
    }
}
