using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.UpdateAdminClinicBed;

public sealed class UpdateAdminClinicBedHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Bed> bedRepository,
    IRepository<Room> roomRepository,
    IRepository<Ward> wardRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateAdminClinicBedCommand, AdminClinicBedDto?>
{
    public async Task<AdminClinicBedDto?> Handle(UpdateAdminClinicBedCommand request, CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService, clinicStaffMembershipService, roleAssignmentService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can update beds.");

        var bed = await bedRepository.GetByIdAsync(request.BedId, cancellationToken).ConfigureAwait(false);
        if (bed is null)
            return null;

        var room = await roomRepository.GetByIdAsync(bed.RoomId, cancellationToken).ConfigureAwait(false);
        if (room is null)
            return null;

        var ward = await wardRepository.GetByIdAsync(room.WardId, cancellationToken).ConfigureAwait(false);
        if (ward is null || ward.ClinicId != request.ClinicId)
            return null;

        if (!request.IsActive && bed.Status == "Occupied")
            throw new BusinessRuleException("Cannot deactivate an occupied bed. Discharge or transfer the patient first.");

        bed.Label = request.Label.Trim();
        bed.IsActive = request.IsActive;

        await bedRepository.UpdateAsync(bed, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new AdminClinicBedDto
        {
            Id = bed.Id,
            RoomId = bed.RoomId,
            Label = bed.Label,
            Status = bed.Status,
            IsActive = bed.IsActive
        };
    }
}
