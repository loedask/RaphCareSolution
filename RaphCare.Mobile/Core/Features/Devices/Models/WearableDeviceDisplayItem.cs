namespace RaphCare.Mobile.Core.Features.Devices.Models;

/// <summary>BLE peripheral shown in the scan list (Plugin.BLE <c>IDevice</c>).</summary>
public sealed class WearableDeviceDisplayItem
{
    public required Guid Id { get; init; }
    public string? Name { get; init; }
    public int? Rssi { get; init; }
}
