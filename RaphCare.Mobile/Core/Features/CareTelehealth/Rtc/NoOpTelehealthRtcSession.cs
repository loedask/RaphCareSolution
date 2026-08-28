namespace RaphCare.Mobile.Core.Features.CareTelehealth.Rtc;

/// <summary>Placeholder when native Agora is not available on this platform.</summary>
public sealed class NoOpTelehealthRtcSession : ITelehealthRtcSession
{
    public event EventHandler? ChannelJoined { add { } remove { } }
    public event EventHandler<int>? RemoteUserJoined { add { } remove { } }
    public event EventHandler<int>? RemoteUserLeft { add { } remove { } }

    public bool IsActive { get; private set; }

    public void BindVideoSurfaces(object? localSurface, object? remoteSurface)
    {
    }

    public Task<TelehealthRtcStartResult> StartAsync(TelehealthRtcJoinParameters parameters, CancellationToken cancellationToken = default) =>
        Task.FromResult(new TelehealthRtcStartResult(
            false,
            "Native Agora video runs on Android. On iOS, run tools/download-agora-ios-framework.ps1, build on a Mac, and use AgoraIosTelehealthRtcSession."));

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        IsActive = false;
        return Task.CompletedTask;
    }

    public Task SetMicrophoneMutedAsync(bool muted, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task SetCameraEnabledAsync(bool enabled, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
