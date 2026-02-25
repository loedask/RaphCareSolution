using MediatR;

namespace RaphCare.Application.Features.Devices.Commands.CreateDevice;

public class CreateDeviceCommand : IRequest<Guid>
{
    public Guid ClinicId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public Guid DeviceTypeId { get; set; }
    public Guid DeviceManufacturerId { get; set; }
}

