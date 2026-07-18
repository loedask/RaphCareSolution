using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicRoom;

public sealed class CreateAdminClinicRoomCommand : IRequest<AdminClinicRoomDto?>
{
    public Guid ClinicId { get; set; }
    public Guid WardId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? RoomType { get; set; }
}
