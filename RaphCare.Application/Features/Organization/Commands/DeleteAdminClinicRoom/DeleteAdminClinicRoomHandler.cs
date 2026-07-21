using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.DeleteAdminClinicRoom;

public sealed class DeleteAdminClinicRoomHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Room> roomRepository,
    IRepository<Ward> wardRepository,
    IRepository<Bed> bedRepository,
    IRepository<InpatientAdmission> admissionRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteAdminClinicRoomCommand, bool>
{
    public async Task<bool> Handle(DeleteAdminClinicRoomCommand request, CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService, clinicStaffMembershipService, roleAssignmentService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can delete rooms.");

        var room = await roomRepository.GetByIdAsync(request.RoomId, cancellationToken).ConfigureAwait(false);
        if (room is null)
            return false;

        var ward = await wardRepository.GetByIdAsync(room.WardId, cancellationToken).ConfigureAwait(false);
        if (ward is null || ward.ClinicId != request.ClinicId)
            return false;

        var beds = await bedRepository.SearchAsync(
            q => q.Where(b => b.RoomId == room.Id),
            1, 500, applyDefaultIdOrdering: false, cancellationToken).ConfigureAwait(false);

        var bedIds = beds.Items.Select(b => b.Id).ToList();
        if (bedIds.Count > 0)
        {
            var active = await admissionRepository.SearchAsync(
                q => q.Where(a => bedIds.Contains(a.BedId) && a.Status == "Admitted"),
                1, 1, applyDefaultIdOrdering: false, cancellationToken).ConfigureAwait(false);
            if (active.TotalCount > 0)
                throw new BusinessRuleException("Cannot delete a room with active admissions. Discharge patients first.");
        }

        var hasHistory = bedIds.Count > 0 && (await admissionRepository.SearchAsync(
            q => q.Where(a => bedIds.Contains(a.BedId)),
            1, 1, applyDefaultIdOrdering: false, cancellationToken).ConfigureAwait(false)).TotalCount > 0;

        if (hasHistory)
        {
            foreach (var bed in beds.Items)
            {
                bed.IsActive = false;
                if (bed.Status != "Occupied")
                    bed.Status = "Available";
                await bedRepository.UpdateAsync(bed, cancellationToken).ConfigureAwait(false);
            }

            room.IsActive = false;
            await roomRepository.UpdateAsync(room, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            foreach (var bed in beds.Items)
                await bedRepository.DeleteAsync(bed, cancellationToken).ConfigureAwait(false);
            await roomRepository.DeleteAsync(room, cancellationToken).ConfigureAwait(false);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
