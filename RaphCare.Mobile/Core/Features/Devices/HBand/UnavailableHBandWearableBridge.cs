using RaphCare.Mobile.Core.Features.Devices.Models;

namespace RaphCare.Mobile.Core.Features.Devices.HBand;

/// <summary>No-op HBand bridge used when the vendor Android SDK is not present or not on this platform.</summary>
public sealed class UnavailableHBandWearableBridge : IHBandWearableBridge
{
    public bool IsAvailable => false;
    public bool IsSessionReady => false;
    public string? ConnectedMacAddress => null;

    public event EventHandler<WearableVitalsSnapshot>? VitalsUpdated
    {
        add { }
        remove { }
    }

    public event EventHandler<string?>? ErrorOccurred
    {
        add { }
        remove { }
    }

    public Task WarmUpAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task ConnectAndHandshakeAsync(
        string macAddress,
        string? deviceName,
        string devicePassword,
        CancellationToken cancellationToken = default) =>
        Task.FromException(new NotSupportedException("HBand vendor SDK is not available on this platform build."));

    public Task StartLiveHeartRateAsync(CancellationToken cancellationToken = default) =>
        Task.FromException(new NotSupportedException("HBand vendor SDK is not available on this platform build."));

    public Task StartLiveSpo2Async(CancellationToken cancellationToken = default) =>
        Task.FromException(new NotSupportedException("HBand vendor SDK is not available on this platform build."));

    public Task StopLiveDetectionsAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task DisconnectAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
