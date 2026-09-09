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
    /// <summary>Optional Bluetooth MAC when known at stock time. Required for new stock; restore of a retired serial may keep a stored MAC if the form omits it. Active duplicates are rejected (use Save MAC on the stock row to correct).</summary>
    public string? BluetoothMacAddress { get; set; }
}
