using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.DeleteAdminClinicBed;

public sealed class DeleteAdminClinicBedHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Bed> bedRepository,
    IRepository<Room> roomRepository,
    IRepository<Ward> wardRepository,
    IRepository<InpatientAdmission> admissionRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteAdminClinicBedCommand, bool>
{
    public async Task<bool> Handle(DeleteAdminClinicBedCommand request, CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService, clinicStaffMembershipService, roleAssignmentService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can delete beds.");

        var bed = await bedRepository.GetByIdAsync(request.BedId, cancellationToken).ConfigureAwait(false);
        if (bed is null)
            return false;

        var room = await roomRepository.GetByIdAsync(bed.RoomId, cancellationToken).ConfigureAwait(false);
        if (room is null)
            return false;

        var ward = await wardRepository.GetByIdAsync(room.WardId, cancellationToken).ConfigureAwait(false);
        if (ward is null || ward.ClinicId != request.ClinicId)
            return false;

        if (bed.Status == "Occupied")
            throw new BusinessRuleException("Cannot delete an occupied bed. Discharge or transfer the patient first.");

        var active = await admissionRepository.SearchAsync(
            q => q.Where(a => a.BedId == bed.Id && a.Status == "Admitted"),
            1, 1, applyDefaultIdOrdering: false, cancellationToken).ConfigureAwait(false);
        if (active.TotalCount > 0)
            throw new BusinessRuleException("Cannot delete a bed with an active admission.");

        // Historical admissions may still reference this bed — block hard delete if any rows exist.
        var anyHistory = await admissionRepository.SearchAsync(
            q => q.Where(a => a.BedId == bed.Id),
            1, 1, applyDefaultIdOrdering: false, cancellationToken).ConfigureAwait(false);
        if (anyHistory.TotalCount > 0)
        {
            bed.IsActive = false;
            if (bed.Status != "Occupied")
                bed.Status = "Available";
            await bedRepository.UpdateAsync(bed, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await bedRepository.DeleteAsync(bed, cancellationToken).ConfigureAwait(false);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
