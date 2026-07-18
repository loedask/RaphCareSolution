using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicBed;

public sealed class CreateAdminClinicBedHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Room> roomRepository,
    IRepository<Ward> wardRepository,
    IRepository<Bed> bedRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAdminClinicBedCommand, AdminClinicBedDto?>
{
    public async Task<AdminClinicBedDto?> Handle(CreateAdminClinicBedCommand request, CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService, clinicStaffMembershipService, roleAssignmentService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can create beds.");

        var room = await roomRepository.GetByIdAsync(request.RoomId, cancellationToken).ConfigureAwait(false);
        if (room is null)
            return null;

        var ward = await wardRepository.GetByIdAsync(room.WardId, cancellationToken).ConfigureAwait(false);
        if (ward is null || ward.ClinicId != request.ClinicId)
            return null;

        var bed = new Bed
        {
            RoomId = room.Id,
            Label = request.Label.Trim(),
            Status = "Available",
            IsActive = true
        };

        await bedRepository.AddAsync(bed, cancellationToken).ConfigureAwait(false);
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
