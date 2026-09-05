using MediatR;
using RaphCare.Application.Features.PatientDevices.DTOs;

namespace RaphCare.Application.Features.PatientDevices.Commands.BindMyDeviceBluetoothMac;

/// <summary>Locks or confirms the Bluetooth MAC for a watch the current patient has claimed.</summary>
public sealed class BindMyDeviceBluetoothMacCommand : IRequest<BindMyDeviceBluetoothMacResponseDto>
{
    public Guid DeviceId { get; set; }
    public string BluetoothMacAddress { get; set; } = string.Empty;
}
