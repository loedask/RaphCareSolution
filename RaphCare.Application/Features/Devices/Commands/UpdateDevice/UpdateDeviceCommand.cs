using MediatR;

namespace RaphCare.Application.Features.Devices.Commands.UpdateDevice;

public class UpdateDeviceCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsAssigned { get; set; }
    public string? Status { get; set; }
}

