namespace RaphCare.Mobile.Core.Features.CareTelehealth.Rtc;

/// <summary>Placeholder when native Agora is not available on this platform.</summary>
public sealed class NoOpTelehealthRtcSession : ITelehealthRtcSession
{
    public void BindVideoSurfaces(object? localSurface, object? remoteSurface)
    {
    }

    public Task<TelehealthRtcStartResult> StartAsync(TelehealthRtcJoinParameters parameters, CancellationToken cancellationToken = default) =>
        Task.FromResult(new TelehealthRtcStartResult(false, "Native Agora video is only enabled on Android in this build."));

    public Task StopAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
