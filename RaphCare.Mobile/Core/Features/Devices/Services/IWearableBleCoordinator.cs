using Microsoft.Maui.ApplicationModel;
using RaphCare.Mobile.Core.Features.Devices.Models;

namespace RaphCare.Mobile.Core.Features.Devices.Services;

/// <summary>Scan, connect, and subscribe to E580/E585-class BLE wearables (Plugin.BLE).</summary>
public interface IWearableBleCoordinator
{
    bool IsBleSupported { get; }
    bool IsScanning { get; }
    Guid? ConnectedDeviceId { get; }
    IReadOnlyList<WearableDeviceDisplayItem> DiscoveredDevices { get; }

    event EventHandler? DiscoveredDevicesChanged;
    event EventHandler<WearableVitalsSnapshot>? VitalsUpdated;
    event EventHandler<string?>? ErrorOccurred;

    Task<PermissionStatus> RequestBluetoothPermissionsAsync(CancellationToken cancellationToken = default);
    Task<bool> EnsureBluetoothAdapterOnAsync(CancellationToken cancellationToken = default);

    Task StartScanAsync(bool showAllDevices, CancellationToken cancellationToken = default);
    Task StopScanAsync();
    Task ConnectAsync(Guid deviceId, CancellationToken cancellationToken = default);
    Task DisconnectAsync(CancellationToken cancellationToken = default);
}
