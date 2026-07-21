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
    /// Connect by BLE MAC, wait for notify, confirm device password, sync default person info.
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

    Task DisconnectAsync(CancellationToken cancellationToken = default);
}
