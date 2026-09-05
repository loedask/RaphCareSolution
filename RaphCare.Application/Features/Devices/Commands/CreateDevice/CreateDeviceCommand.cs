using MediatR;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Devices.Commands.CreateDevice;

/// <summary>Adds a wearable to RaphCare fleet inventory (unassigned stock).</summary>
public class CreateDeviceCommand : IRequest<Guid>, IPlatformAdminRequest
{
    public Guid ClinicId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public Guid DeviceTypeId { get; set; }
    public Guid DeviceManufacturerId { get; set; }
    /// <summary>Optional Bluetooth MAC when known at stock time.</summary>
    public string? BluetoothMacAddress { get; set; }
}
