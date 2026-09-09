namespace RaphCare.Mobile.Core.Features.Devices.Models;

/// <summary>Row bound on the devices page for a discovered BLE peripheral.</summary>
public sealed class WearableDeviceRowViewModel
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string RssiText { get; init; }

    /// <summary>True when this row is the active GATT/HBand session.</summary>
    public bool IsConnected { get; init; }

    /// <summary>Connect or Connected.</summary>
    public required string ActionLabel { get; init; }

    /// <summary>False while this row is already the connected peripheral.</summary>
    public bool CanConnect => !IsConnected;
}
