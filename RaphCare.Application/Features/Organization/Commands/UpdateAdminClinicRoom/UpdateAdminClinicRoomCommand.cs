using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.UpdateAdminClinicRoom;

public sealed class UpdateAdminClinicRoomCommand : IRequest<AdminClinicRoomDto?>
{
    public Guid ClinicId { get; set; }
    public Guid RoomId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? RoomType { get; set; }
    public bool IsActive { get; set; } = true;
}
