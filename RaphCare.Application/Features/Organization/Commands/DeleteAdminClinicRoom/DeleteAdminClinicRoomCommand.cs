using MediatR;

namespace RaphCare.Application.Features.Organization.Commands.DeleteAdminClinicRoom;

public sealed class DeleteAdminClinicRoomCommand : IRequest<bool>
{
    public Guid ClinicId { get; set; }
    public Guid RoomId { get; set; }
}
