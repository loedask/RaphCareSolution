using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Identity;

namespace RaphCare.Application.Features.Organization.Commands.UpdateClinicStaffRole;

public sealed class UpdateClinicStaffRoleHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService)
    : IRequestHandler<UpdateClinicStaffRoleCommand, bool>
{
    public async Task<bool> Handle(UpdateClinicStaffRoleCommand request, CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can change staff roles.");

        if (currentUserService.CurrentUserId == request.UserId)
            throw new BusinessRuleException("You cannot change your own administrator role.");

        if (!await clinicStaffMembershipService
                .HasMembershipAsync(request.UserId, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            return false;

        var roles = await roleAssignmentService
            .GetRoleNamesAsync(request.UserId, cancellationToken)
            .ConfigureAwait(false);
        var isCurrentlyAdministrator = RaphCareRoles.HasAdministratorRole(roles);

        if (request.JobRole is not null)
        {
            await roleAssignmentService
                .SetStaffJobRoleAsync(request.UserId, request.JobRole, cancellationToken)
                .ConfigureAwait(false);
        }

        if (request.IsAdministrator)
        {
            if (!isCurrentlyAdministrator)
            {
                if (!RaphCareRoles.HasProviderJobRole(
                        await roleAssignmentService.GetRoleNamesAsync(request.UserId, cancellationToken)
                            .ConfigureAwait(false)))
                {
                    await roleAssignmentService
                        .AssignRoleIfMissingAsync(request.UserId, RaphCareRoles.Clinician, cancellationToken)
                        .ConfigureAwait(false);
                }

                await roleAssignmentService
                    .AssignRoleIfMissingAsync(request.UserId, RaphCareRoles.Administrator, cancellationToken)
                    .ConfigureAwait(false);
            }

            return true;
        }

        if (!isCurrentlyAdministrator)
            return true;

        if (!await AdminClinicStaffRules.HasAnotherClinicAdministratorAsync(
                request.ClinicId,
                request.UserId,
                clinicStaffMembershipService,
                roleAssignmentService,
                cancellationToken)
            .ConfigureAwait(false))
            throw new BusinessRuleException("At least one other administrator must remain for this hospital.");

        await roleAssignmentService
            .RemoveRoleAsync(request.UserId, RaphCareRoles.Administrator, cancellationToken)
            .ConfigureAwait(false);

        if (!RaphCareRoles.HasProviderJobRole(
                await roleAssignmentService.GetRoleNamesAsync(request.UserId, cancellationToken)
                    .ConfigureAwait(false)))
        {
            await roleAssignmentService
                .AssignRoleIfMissingAsync(request.UserId, RaphCareRoles.Clinician, cancellationToken)
                .ConfigureAwait(false);
        }

        return true;
    }
}
