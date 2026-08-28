using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.UpdateAdminClinicRoom;

public sealed class UpdateAdminClinicRoomHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Room> roomRepository,
    IRepository<Ward> wardRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateAdminClinicRoomCommand, AdminClinicRoomDto?>
{
    public async Task<AdminClinicRoomDto?> Handle(UpdateAdminClinicRoomCommand request, CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService, clinicStaffMembershipService, roleAssignmentService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can update rooms.");

        var room = await roomRepository.GetByIdAsync(request.RoomId, cancellationToken).ConfigureAwait(false);
        if (room is null)
            return null;

        var ward = await wardRepository.GetByIdAsync(room.WardId, cancellationToken).ConfigureAwait(false);
        if (ward is null || ward.ClinicId != request.ClinicId)
            return null;

        room.Name = request.Name.Trim();
        room.RoomType = string.IsNullOrWhiteSpace(request.RoomType) ? null : request.RoomType.Trim();
        room.IsActive = request.IsActive;

        await roomRepository.UpdateAsync(room, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new AdminClinicRoomDto
        {
            Id = room.Id,
            WardId = room.WardId,
            Name = room.Name,
            RoomType = room.RoomType,
            IsActive = room.IsActive,
            Beds = Array.Empty<AdminClinicBedDto>()
        };
    }
}
