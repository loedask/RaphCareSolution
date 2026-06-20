using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.RemoveClinicStaff;

public sealed class RemoveClinicStaffHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Clinic> clinicRepository)
    : IRequestHandler<RemoveClinicStaffCommand, bool>
{
    public async Task<bool> Handle(RemoveClinicStaffCommand request, CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            return false;

        var clinic = await clinicRepository.GetByIdAsync(request.ClinicId, cancellationToken).ConfigureAwait(false);
        if (clinic is null || clinic.IsDeleted)
            return false;

        await AdminClinicStaffRules.ValidateStaffRemovalAsync(
            clinic,
            request.UserId,
            currentUserService.CurrentUserId,
            clinicStaffMembershipService,
            roleAssignmentService,
            cancellationToken).ConfigureAwait(false);

        return await clinicStaffMembershipService
            .DeactivateMembershipAsync(request.UserId, request.ClinicId, cancellationToken)
            .ConfigureAwait(false);
    }
}
