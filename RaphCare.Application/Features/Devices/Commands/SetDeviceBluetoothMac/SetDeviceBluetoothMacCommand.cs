using MediatR;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Devices.Commands.SetDeviceBluetoothMac;

/// <summary>Sets or updates the Bluetooth MAC on an active fleet device (platform ops).</summary>
public sealed class SetDeviceBluetoothMacCommand : IRequest, IPlatformAdminRequest
{
    public Guid DeviceId { get; set; }
    public string BluetoothMacAddress { get; set; } = string.Empty;
}
