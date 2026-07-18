using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicBed;

public sealed class CreateAdminClinicBedCommand : IRequest<AdminClinicBedDto?>
{
    public Guid ClinicId { get; set; }
    public Guid RoomId { get; set; }
    public string Label { get; set; } = string.Empty;
}
