using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.DeleteAdminFacility;

public sealed class DeleteAdminFacilityHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Facility> facilityRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteAdminFacilityCommand, bool>
{
    public async Task<bool> Handle(DeleteAdminFacilityCommand request, CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            return false;

        var facility = await facilityRepository.GetByIdAsync(request.FacilityId, cancellationToken).ConfigureAwait(false);
        if (facility is null || facility.ClinicId != request.ClinicId)
            return false;

        await facilityRepository.DeleteAsync(facility, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
