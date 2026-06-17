namespace RaphCare.Mobile.Core.Features.CareTelehealth.Rtc;

/// <summary>
/// Native Agora RTC session (no Agora pre-built UI). Android pulls <c>io.agora.rtc:full-sdk</c> from Maven and binds views via <c>BindVideoSurfaces</c>.
/// Other platforms: use <see cref="NoOpTelehealthRtcSession"/> until native SDKs are wired.
/// </summary>
public interface ITelehealthRtcSession
{
    /// <summary>Android: pass <c>TextureView</c> instances from <c>TelehealthVideoView</c> handlers. Other platforms: ignored.</summary>
    void BindVideoSurfaces(object? localSurface, object? remoteSurface);

    Task<TelehealthRtcStartResult> StartAsync(TelehealthRtcJoinParameters parameters, CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);
}
