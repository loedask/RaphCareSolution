using Android.Util;
using Android.Views;
using Java.Lang;
using Java.Lang.Reflect;
using Microsoft.Maui.ApplicationModel;
using RaphCare.Mobile.Core.Features.CareTelehealth.Rtc;

namespace RaphCare.Mobile.Platforms.Android.Telehealth;

/// <summary>
/// Joins an Agora channel using Maven <c>io.agora.rtc:full-sdk</c> (no Agora UI kit).
/// Uses <see cref="Java.Lang.Reflect"/> because <c>AndroidJavaObject</c> is not exposed on net10 reference assemblies.
/// </summary>
public sealed class AgoraAndroidTelehealthRtcSession : ITelehealthRtcSession
{
    private readonly object _sync = new();
    private Java.Lang.Object? _engine;
    private TextureView? _local;
    private TextureView? _remote;
    private AgoraInvocationHandler? _handler;

    public void BindVideoSurfaces(object? localSurface, object? remoteSurface)
    {
        lock (_sync)
        {
            _local = localSurface as TextureView;
            _remote = remoteSurface as TextureView;
        }
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

        return await MainThread.InvokeOnMainThreadAsync(() => StartCore(parameters)).ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken = default) =>
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            lock (_sync)
                InternalStopLocked();
        });

    private TelehealthRtcStartResult StartCore(TelehealthRtcJoinParameters parameters)
    {
        lock (_sync)
        {
            try
            {
                InternalStopLocked();

                var configClass = Class.ForName("io.agora.rtc2.RtcEngineConfig");
                var configCtor = configClass.GetConstructor([]);
                var config = configCtor.NewInstance([]);
                configClass.GetField("mContext")!.Set(config, global::Android.App.Application.Context);
                configClass.GetField("mAppId")!.Set(config, new Java.Lang.String(parameters.AppId));

                _handler = new AgoraInvocationHandler(this);
                var iface = Class.ForName("io.agora.rtc2.IRtcEngineEventHandler");
                var loader = iface.ClassLoader ?? throw new InvalidOperationException("Missing class loader for Agora.");
                var proxy = Proxy.NewProxyInstance(loader, [iface], _handler);
                configClass.GetField("mEventHandler")!.Set(config, proxy);

                var rtcClass = Class.ForName("io.agora.rtc2.RtcEngine");
                var create = rtcClass.GetDeclaredMethod("create", configClass);
                _engine = create.Invoke(null, new Java.Lang.Object[] { config! }) as Java.Lang.Object;
                if (_engine is null)
                    return new TelehealthRtcStartResult(false, "RtcEngine.create returned null.");

                InvokeNoArgInt(_engine, "enableVideo");
                InvokeNoArgInt(_engine, "startPreview");

                if (_local is not null)
                {
                    var canvasClass = Class.ForName("io.agora.rtc2.video.VideoCanvas");
                    var viewClass = Class.ForName("android.view.View");
                    var ctor = canvasClass.GetConstructor(viewClass, Integer.Type, Integer.Type);
                    var canvas = ctor.NewInstance(_local, Integer.ValueOf(1), Integer.ValueOf(0));
                    var setup = _engine.Class.GetMethod("setupLocalVideo", canvasClass);
                    setup.Invoke(_engine, canvas);
                }

                var optionsClass = Class.ForName("io.agora.rtc2.ChannelMediaOptions");
                var optionsCtor = optionsClass.GetConstructor([]);
                var options = optionsCtor.NewInstance([]);
                InvokeOptionsBool(options, "setAutoSubscribeAudio", true);
                InvokeOptionsBool(options, "setAutoSubscribeVideo", true);
                InvokeOptionsBool(options, "setPublishCameraTrack", true);
                InvokeOptionsBool(options, "setPublishMicrophoneTrack", true);
                InvokeOptionsInt(options, "setClientRoleType", 1);

                var join = FindJoinChannelMethod(_engine.Class, optionsClass);
                if (join is null)
                {
                    InternalStopLocked();
                    return new TelehealthRtcStartResult(false, "joinChannel overload not found.");
                }

                var token = new Java.Lang.String(parameters.Token ?? string.Empty);
                var channel = new Java.Lang.String(parameters.ChannelName);
                var uidBox = Integer.ValueOf(parameters.Uid);
                var codeObj = join.Invoke(_engine, token, channel, uidBox, options);
                var code = UnboxInt(codeObj);
                if (code != 0)
                {
                    InternalStopLocked();
                    return new TelehealthRtcStartResult(false, $"joinChannel failed (code {code}).");
                }

                return new TelehealthRtcStartResult(true, null);
            }
            catch (Throwable t)
            {
                Log.Error("RaphCareRtc", t.ToString());
                InternalStopLocked();
                return new TelehealthRtcStartResult(false, t.Message ?? "Agora start failed.");
            }
            catch (System.Exception ex)
            {
                Log.Error("RaphCareRtc", ex.ToString());
                InternalStopLocked();
                return new TelehealthRtcStartResult(false, ex.Message);
            }
        }
    }

    private static Method? FindJoinChannelMethod(Class rtcClass, Class optionsClass)
    {
        foreach (var m in rtcClass.GetMethods()!)
        {
            if (m.Name != "joinChannel")
                continue;
            var p = m.GetParameterTypes();
            if (p is null || p.Length != 4)
                continue;
            if (p[0].Name != "java.lang.String" || p[1].Name != "java.lang.String")
                continue;
            if (p[3].Name != optionsClass.Name)
                continue;
            return m;
        }

        return null;
    }

    private static void InvokeNoArgInt(Java.Lang.Object target, string name)
    {
        var m = target.Class.GetMethod(name, []);
        m.Invoke(target, []);
    }

    private static void InvokeOptionsBool(Java.Lang.Object options, string setter, bool value)
    {
        var m = options.Class.GetMethod(setter, [Java.Lang.Boolean.Type]);
        m.Invoke(options, [Java.Lang.Boolean.ValueOf(value)]);
    }

    private static void InvokeOptionsInt(Java.Lang.Object options, string setter, int value)
    {
        var m = options.Class.GetMethod(setter, [Integer.Type]);
        m.Invoke(options, [Integer.ValueOf(value)]);
    }

    private static int UnboxInt(Java.Lang.Object? boxed) =>
        boxed is Integer ji ? ji.IntValue() : 0;

    private void InternalStopLocked()
    {
        try
        {
            if (_engine is not null)
            {
                TryInvokeNoArg(_engine, "stopPreview");
                TryInvokeNoArg(_engine, "leaveChannel");
            }
        }
        catch (System.Exception ex)
        {
            Log.Warn("RaphCareRtc", ex.ToString());
        }
        finally
        {
            _engine = null;
            _handler?.Dispose();
            _handler = null;

            try
            {
                var rtcClass = Class.ForName("io.agora.rtc2.RtcEngine");
                var destroy = rtcClass.GetDeclaredMethod("destroy");
                destroy.Invoke(null, null);
            }
            catch (System.Exception ex)
            {
                Log.Warn("RaphCareRtc", ex.ToString());
            }
        }
    }

    private static void TryInvokeNoArg(Java.Lang.Object target, string name)
    {
        try
        {
            var m = target.Class.GetMethod(name, []);
            m.Invoke(target, []);
        }
        catch
        {
            // Method signature may differ by SDK; ignore on teardown.
        }
    }

    internal void OnRemoteUserJoined(int remoteUid)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            TextureView? remote;
            Java.Lang.Object? engine;
            lock (_sync)
            {
                remote = _remote;
                engine = _engine;
            }

            if (remote is null || engine is null)
                return;

            try
            {
                var canvasClass = Class.ForName("io.agora.rtc2.video.VideoCanvas");
                var viewClass = Class.ForName("android.view.View");
                var ctor = canvasClass.GetConstructor(viewClass, Integer.Type, Integer.Type);
                var canvas = ctor.NewInstance(remote, Integer.ValueOf(1), Integer.ValueOf(remoteUid));
                var setup = engine.Class.GetMethod("setupRemoteVideo", canvasClass);
                setup.Invoke(engine, canvas);
            }
            catch (System.Exception ex)
            {
                Log.Warn("RaphCareRtc", ex.ToString());
            }
        });
    }

    private sealed class AgoraInvocationHandler : Java.Lang.Object, IInvocationHandler
    {
        private readonly AgoraAndroidTelehealthRtcSession _owner;

        public AgoraInvocationHandler(AgoraAndroidTelehealthRtcSession owner) => _owner = owner;

        public Java.Lang.Object? Invoke(Java.Lang.Object? proxy, Method? method, Java.Lang.Object[]? args)
        {
            var name = method?.Name;
            if (name == "onUserJoined" && args is { Length: >= 1 })
            {
                var uid = args[0] is Integer ji ? ji.IntValue() : 0;
                _owner.OnRemoteUserJoined(uid);
            }
            else if (name == "onError" && args is { Length: >= 1 })
            {
                var err = args[0] is Integer ei ? ei.IntValue() : 0;
                Log.Warn("RaphCareRtc", $"Agora onError code={err}");
            }

            return null;
        }
    }
}
