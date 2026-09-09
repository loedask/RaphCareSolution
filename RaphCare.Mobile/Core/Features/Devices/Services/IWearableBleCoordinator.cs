using Microsoft.Maui.ApplicationModel;
using RaphCare.Mobile.Core.Features.Devices.Models;

namespace RaphCare.Mobile.Core.Features.Devices.Services;

/// <summary>Scan, connect, and subscribe to E580/E585-class BLE wearables (Plugin.BLE + optional HBand measure).</summary>
public interface IWearableBleCoordinator
{
    bool IsBleSupported { get; }
    bool IsScanning { get; }
    Guid? ConnectedDeviceId { get; }

    /// <summary>True when the vendor HBand bridge is present (Android AARs).</summary>
    bool IsVendorMeasureAvailable { get; }

    /// <summary>
    /// True when Watch readings Measure has an exclusive vendor session ready
    /// (Devices "Connected" alone is not enough when PreferExclusiveVendorSession is on).
    /// </summary>
    bool IsLiveMeasureSessionReady { get; }

    /// <summary>Last merged vitals from GATT notify or a live Measure.</summary>
    WearableVitalsSnapshot? LastVitals { get; }

    IReadOnlyList<WearableDeviceDisplayItem> DiscoveredDevices { get; }

    event EventHandler? DiscoveredDevicesChanged;
    event EventHandler<WearableVitalsSnapshot>? VitalsUpdated;
    event EventHandler<string?>? ErrorOccurred;

    Task<PermissionStatus> RequestBluetoothPermissionsAsync(CancellationToken cancellationToken = default);

    /// <summary>Check Nearby devices / Bluetooth permission without showing a dialog.</summary>
    Task<PermissionStatus> CheckBluetoothPermissionsAsync(CancellationToken cancellationToken = default);

    Task<bool> EnsureBluetoothAdapterOnAsync(CancellationToken cancellationToken = default);

    Task StartScanAsync(
        bool showAllDevices,
        string? preferredMacAddress = null,
        CancellationToken cancellationToken = default);
    Task StopScanAsync();
    Task ConnectAsync(Guid deviceId, CancellationToken cancellationToken = default);
    Task DisconnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reconnect the claimed watch by Bluetooth address after the app process was killed.
    /// Does not require a prior Scan when the vendor SDK can connect by MAC.
    /// </summary>
    Task ReconnectClaimedWatchAsync(
        string macAddress,
        string? deviceName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs a vendor live measure (heart rate, then SpO₂ when available).
    /// E580/E585 on-watch Health Monitor does not push SIG GATT vitals; this is the phone path that can read them.
    /// </summary>
    Task<WearableVitalsSnapshot?> MeasureLiveVitalsAsync(CancellationToken cancellationToken = default);
}
