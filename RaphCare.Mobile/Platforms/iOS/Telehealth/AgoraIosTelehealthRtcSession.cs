using ObjCRuntime;
using RaphCare.Mobile.Core.Features.CareTelehealth.Rtc;

namespace RaphCare.Mobile.Platforms.iOS.Telehealth;

/// <summary>
/// iOS telehealth RTC entry point. Full AgoraRtcKit join requires
/// <c>tools/download-agora-ios-framework.ps1</c> and a Mac build with the xcframework embedded.
/// Until then, requests camera/mic and returns a clear setup message (same bar as Android permissions).
/// </summary>
public sealed class AgoraIosTelehealthRtcSession : ITelehealthRtcSession
{
    private readonly object _sync = new();
    private bool _isActive;

    public event EventHandler? ChannelJoined;
    public event EventHandler<int>? RemoteUserJoined;
    public event EventHandler<int>? RemoteUserLeft;

    // Used once AgoraRtcEngineKit join callbacks are wired (parity with Android session).
    internal void OnChannelJoined() => ChannelJoined?.Invoke(this, EventArgs.Empty);
    internal void OnRemoteUserJoined(int remoteUid) => RemoteUserJoined?.Invoke(this, remoteUid);
    internal void OnRemoteUserLeft(int remoteUid) => RemoteUserLeft?.Invoke(this, remoteUid);

    public bool IsActive
    {
        get
        {
            lock (_sync)
                return _isActive;
        }
    }

    public void BindVideoSurfaces(object? localSurface, object? remoteSurface)
    {
        // Surfaces bind when AgoraRtcKit session is implemented against the xcframework.
    }

    public async Task<TelehealthRtcStartResult> StartAsync(TelehealthRtcJoinParameters parameters, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(parameters.AppId))
            return new TelehealthRtcStartResult(false, "Missing Agora app id.");
        if (string.IsNullOrWhiteSpace(parameters.ChannelName))
            return new TelehealthRtcStartResult(false, "Missing channel name.");

        var cam = await Permissions.RequestAsync<Permissions.Camera>().ConfigureAwait(false);
        var mic = await Permissions.RequestAsync<Permissions.Microphone>().ConfigureAwait(false);
        if (cam != PermissionStatus.Granted || mic != PermissionStatus.Granted)
            return new TelehealthRtcStartResult(false, "Camera and microphone permissions are required for video.");

        if (!IsAgoraFrameworkPresent())
        {
            return new TelehealthRtcStartResult(
                false,
                "AgoraRtcKit is not embedded. On a Mac, run tools/download-agora-ios-framework.ps1 then rebuild net10.0-ios.");
        }

        // Framework present: native join still requires ObjC AgoraRtcEngineKit wiring on a Mac.
        return new TelehealthRtcStartResult(
            false,
            "AgoraRtcKit is present but the native iOS join path is not wired yet. Use Android for live video demos.");
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        lock (_sync)
            _isActive = false;
        return Task.CompletedTask;
    }

    public Task SetMicrophoneMutedAsync(bool muted, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task SetCameraEnabledAsync(bool enabled, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    private static bool IsAgoraFrameworkPresent()
    {
        try
        {
            return Class.GetHandle("AgoraRtcEngineKit") != IntPtr.Zero;
        }
        catch
        {
            return false;
        }
    }
}
