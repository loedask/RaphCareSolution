using RaphCare.Mobile.Core.Features.Devices.Models;

namespace RaphCare.Mobile.Core.Features.Devices.HBand;

/// <summary>
/// Optional vendor (HBand / Veepoo) path for E580/E585-class bands.
/// Android implements this via <c>VPOperateManager</c> JNI; other platforms use a no-op.
/// </summary>
public interface IHBandWearableBridge
{
    /// <summary>True when native SDK classes are on the classpath (Android libs downloaded).</summary>
    bool IsAvailable { get; }

    /// <summary>True after a successful connect + password + person sync handshake.</summary>
    bool IsSessionReady { get; }

    string? ConnectedMacAddress { get; }

    event EventHandler<WearableVitalsSnapshot>? VitalsUpdated;
    event EventHandler<string?>? ErrorOccurred;

    /// <summary>
    /// Fired for each device found during <see cref="StartVendorScanAsync"/>.
    /// Probe mode should pass a MAC from this event into <see cref="ConnectAndHandshakeAsync"/>.
    /// </summary>
    event EventHandler<VendorScanDeviceFoundEventArgs>? VendorScanDeviceFound;

    /// <summary>
    /// Live breadcrumb steps (INIT/SCAN/CONNECT) for on-screen probe progress.
    /// </summary>
    event EventHandler<string>? ConnectStepChanged;

    /// <summary>
    /// Load <c>VPOperateManager</c> and ensure Inuker <c>BluetoothClient</c> exists.
    /// Does not call <c>connectDevice</c>. Safe to run on Devices appear.
    /// </summary>
    Task WarmUpAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Start Veepoo/Inuker scan (<c>startScanDevice</c>). Does not use Plugin.BLE.
    /// </summary>
    Task StartVendorScanAsync(CancellationToken cancellationToken = default);

    /// <summary>Stop Veepoo/Inuker scan (<c>stopScanDevice</c>).</summary>
    Task StopVendorScanAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Connect by BLE MAC, wait for notify, confirm device password, sync default person info.
    /// In vendor-scan probe mode, prefer a MAC obtained from <see cref="VendorScanDeviceFound"/>.
    /// </summary>
    Task ConnectAndHandshakeAsync(
        string macAddress,
        string? deviceName,
        string devicePassword,
        CancellationToken cancellationToken = default);

    /// <summary>Start vendor live heart-rate detection (after handshake).</summary>
    Task StartLiveHeartRateAsync(CancellationToken cancellationToken = default);

    /// <summary>Start vendor SpO₂ detection (after handshake).</summary>
    Task StartLiveSpo2Async(CancellationToken cancellationToken = default);

    /// <summary>Stop in-flight heart / SpO₂ detect without full disconnect.</summary>
    Task StopLiveDetectionsAsync(CancellationToken cancellationToken = default);

    Task DisconnectAsync(CancellationToken cancellationToken = default);
}
