namespace RaphCare.Mobile.Core.Features.CareTelehealth.Rtc;

/// <summary>Native Agora RTC session (no Agora pre-built UI). Android pulls <c>io.agora.rtc:full-sdk</c> from Maven and binds views via <c>BindVideoSurfaces</c>.
/// Other platforms: use <see cref="NoOpTelehealthRtcSession"/> until native SDKs are wired.</summary>
public interface ITelehealthRtcSession
{
    /// <summary>Raised when the local client successfully joins the Agora channel.</summary>
    event EventHandler? ChannelJoined;

    /// <summary>Raised when a remote participant joins (uid).</summary>
    event EventHandler<int>? RemoteUserJoined;

    /// <summary>Raised when a remote participant leaves (uid).</summary>
    event EventHandler<int>? RemoteUserLeft;

    /// <summary>True after a successful <see cref="StartAsync"/> until <see cref="StopAsync"/>.</summary>
    bool IsActive { get; }

    /// <summary>Android: pass <c>TextureView</c> instances from <c>TelehealthVideoView</c> handlers. Other platforms: ignored.</summary>
    void BindVideoSurfaces(object? localSurface, object? remoteSurface);

    Task<TelehealthRtcStartResult> StartAsync(TelehealthRtcJoinParameters parameters, CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);

    Task SetMicrophoneMutedAsync(bool muted, CancellationToken cancellationToken = default);

    Task SetCameraEnabledAsync(bool enabled, CancellationToken cancellationToken = default);
}
