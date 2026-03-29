namespace RaphCare.Mobile.Core.Features.Devices.Models;

/// <summary>Row bound on the devices page for a discovered BLE peripheral.</summary>
public sealed class WearableDeviceRowViewModel
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string RssiText { get; init; }
}
