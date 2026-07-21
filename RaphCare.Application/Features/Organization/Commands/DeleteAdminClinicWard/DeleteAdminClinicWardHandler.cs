using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.DeleteAdminClinicWard;

public sealed class DeleteAdminClinicWardHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Ward> wardRepository,
    IRepository<Room> roomRepository,
    IRepository<Bed> bedRepository,
    IRepository<InpatientAdmission> admissionRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteAdminClinicWardCommand, bool>
{
    public async Task<bool> Handle(DeleteAdminClinicWardCommand request, CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService, clinicStaffMembershipService, roleAssignmentService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can delete wards.");

        var ward = await wardRepository.GetByIdAsync(request.WardId, cancellationToken).ConfigureAwait(false);
        if (ward is null || ward.ClinicId != request.ClinicId)
            return false;

        var rooms = await roomRepository.SearchAsync(
            q => q.Where(r => r.WardId == ward.Id),
            1, 500, applyDefaultIdOrdering: false, cancellationToken).ConfigureAwait(false);

        var roomIds = rooms.Items.Select(r => r.Id).ToList();
        var beds = roomIds.Count == 0
            ? new List<Bed>()
            : (await bedRepository.SearchAsync(
                q => q.Where(b => roomIds.Contains(b.RoomId)),
                1, 2000, applyDefaultIdOrdering: false, cancellationToken).ConfigureAwait(false)).Items.ToList();

        var bedIds = beds.Select(b => b.Id).ToList();
        if (bedIds.Count > 0)
        {
            var active = await admissionRepository.SearchAsync(
                q => q.Where(a => bedIds.Contains(a.BedId) && a.Status == "Admitted"),
                1, 1, applyDefaultIdOrdering: false, cancellationToken).ConfigureAwait(false);
            if (active.TotalCount > 0)
                throw new BusinessRuleException("Cannot delete a ward with active admissions. Discharge patients first.");
        }

        var hasHistory = bedIds.Count > 0 && (await admissionRepository.SearchAsync(
            q => q.Where(a => bedIds.Contains(a.BedId)),
            1, 1, applyDefaultIdOrdering: false, cancellationToken).ConfigureAwait(false)).TotalCount > 0;

        if (hasHistory)
        {
            foreach (var bed in beds)
            {
                bed.IsActive = false;
                if (bed.Status != "Occupied")
                    bed.Status = "Available";
                await bedRepository.UpdateAsync(bed, cancellationToken).ConfigureAwait(false);
            }

            foreach (var room in rooms.Items)
            {
                room.IsActive = false;
                await roomRepository.UpdateAsync(room, cancellationToken).ConfigureAwait(false);
            }

            ward.IsActive = false;
            await wardRepository.UpdateAsync(ward, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            foreach (var bed in beds)
                await bedRepository.DeleteAsync(bed, cancellationToken).ConfigureAwait(false);
            foreach (var room in rooms.Items)
                await roomRepository.DeleteAsync(room, cancellationToken).ConfigureAwait(false);
            await wardRepository.DeleteAsync(ward, cancellationToken).ConfigureAwait(false);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
