using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicRoom;

public sealed class CreateAdminClinicRoomHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Ward> wardRepository,
    IRepository<Room> roomRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAdminClinicRoomCommand, AdminClinicRoomDto?>
{
    public async Task<AdminClinicRoomDto?> Handle(CreateAdminClinicRoomCommand request, CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService, clinicStaffMembershipService, roleAssignmentService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can create rooms.");

        var ward = await wardRepository.GetByIdAsync(request.WardId, cancellationToken).ConfigureAwait(false);
        if (ward is null || ward.ClinicId != request.ClinicId)
            return null;

        var room = new Room
        {
            WardId = ward.Id,
            Name = request.Name.Trim(),
            RoomType = string.IsNullOrWhiteSpace(request.RoomType) ? null : request.RoomType.Trim(),
            IsActive = true
        };

        await roomRepository.AddAsync(room, cancellationToken).ConfigureAwait(false);
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
