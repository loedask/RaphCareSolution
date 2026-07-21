using Android.Util;
using Java.Lang;
using Java.Lang.Reflect;
using Microsoft.Maui.ApplicationModel;
using RaphCare.Mobile.Core.Features.Devices.HBand;
using RaphCare.Mobile.Core.Features.Devices.Models;
using Exception = System.Exception;

namespace RaphCare.Mobile.Platforms.Android.HBand;

/// <summary>
/// Calls Veepoo <c>VPOperateManager</c> via Java reflection (AARs embedded with Bind=false),
/// matching the Agora RTC pattern. Flow: init → connectDevice → notify → confirmDevicePwd → syncPersonInfo → detect HR/SpO₂.
/// </summary>
public sealed class HBandAndroidWearableBridge : IHBandWearableBridge, IDisposable
{
    private const string Tag = "RaphCareHBand";
    private readonly object _sync = new();
    private Java.Lang.Object? _manager;
    private bool _initialized;
    private bool _sessionReady;
    private string? _connectedMac;
    private HBandInvocationHandler? _activeHandler;

    public bool IsAvailable
    {
        get
        {
            try
            {
                Class.ForName("com.veepoo.protocol.VPOperateManager");
                return true;
            }
            catch (Throwable)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public bool IsSessionReady
    {
        get
        {
            lock (_sync)
                return _sessionReady;
        }
    }

    public string? ConnectedMacAddress
    {
        get
        {
            lock (_sync)
                return _connectedMac;
        }
    }

    public event EventHandler<WearableVitalsSnapshot>? VitalsUpdated;
    public event EventHandler<string?>? ErrorOccurred;

    private void RaiseError(string? message) =>
        MainThread.BeginInvokeOnMainThread(() => ErrorOccurred?.Invoke(this, message));

    public Task ConnectAndHandshakeAsync(
        string macAddress,
        string? deviceName,
        string devicePassword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(macAddress))
            throw new ArgumentException("MAC address is required.", nameof(macAddress));

        return MainThread.InvokeOnMainThreadAsync(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureInitialized();
            var manager = _manager ?? throw new InvalidOperationException("VPOperateManager is null.");

            await DisconnectCoreAsync().ConfigureAwait(true);

            var connectTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var notifyTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var reg = cancellationToken.Register(() =>
            {
                connectTcs.TrySetCanceled(cancellationToken);
                notifyTcs.TrySetCanceled(cancellationToken);
            });

            var connectProxy = CreateProxy(
                "com.veepoo.protocol.listener.base.IConnectResponse",
                (method, args) =>
                {
                    if (method.Name is "connectState" or "onResponse")
                    {
                        var code = UnboxInt(args.Length > 0 ? args[0] : null);
                        if (code == RequestSuccessCode())
                            connectTcs.TrySetResult(true);
                        else
                            connectTcs.TrySetException(new InvalidOperationException($"HBand connect failed (code {code})."));
                    }

                    return null;
                });

            var notifyProxy = CreateProxy(
                "com.veepoo.protocol.listener.base.INotifyResponse",
                (method, args) =>
                {
                    if (method.Name is "notifyState" or "onResponse" or "notifySuccess")
                    {
                        var code = args.Length > 0 ? UnboxInt(args[0]) : RequestSuccessCode();
                        if (code == RequestSuccessCode() || method.Name == "notifySuccess")
                            notifyTcs.TrySetResult(true);
                        else
                            notifyTcs.TrySetException(new InvalidOperationException($"HBand notify failed (code {code})."));
                    }

                    return null;
                });

            var mac = new Java.Lang.String(macAddress);
            var name = new Java.Lang.String(deviceName ?? string.Empty);

            // Preferred: connectDevice(mac, name, connectResponse, notifyResponse)
            var connected = TryInvoke(manager, "connectDevice", mac, name, connectProxy, notifyProxy);
            if (!connected)
            {
                // Fallback: connectDevice(mac, connectResponse, notifyResponse)
                if (!TryInvoke(manager, "connectDevice", mac, connectProxy, notifyProxy))
                    throw new InvalidOperationException("connectDevice overload not found on VPOperateManager.");
            }

            await connectTcs.Task.ConfigureAwait(true);
            await notifyTcs.Task.ConfigureAwait(true);

            await ConfirmPasswordAsync(manager, devicePassword, cancellationToken).ConfigureAwait(true);
            await SyncPersonInfoAsync(manager, cancellationToken).ConfigureAwait(true);

            lock (_sync)
            {
                _connectedMac = macAddress;
                _sessionReady = true;
            }
        });
    }

    public Task StartLiveHeartRateAsync(CancellationToken cancellationToken = default) =>
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureSession();
            var manager = _manager!;
            var write = CreateWriteResponseProxy();
            var heartProxy = CreateProxy(
                "com.veepoo.protocol.listener.data.IHeartDataListener",
                (method, args) =>
                {
                    if (method.Name != "onDataChange" || args.Length == 0 || args[0] is null)
                        return null;

                    try
                    {
                        var bpm = ReadIntProperty(args[0]!, "getData", "data");
                        if (bpm is >= 20 and <= 300)
                        {
                            RaiseVitals(new WearableVitalsSnapshot
                            {
                                At = DateTimeOffset.UtcNow,
                                HeartRateBpm = bpm,
                                CharacteristicUuid = "hband:heart",
                                RawHex = $"hr={bpm}"
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warn(Tag, "Heart parse failed: " + ex.Message);
                    }

                    return null;
                });

            if (!TryInvoke(manager, "startDetectHeart", write, heartProxy))
                throw new InvalidOperationException("startDetectHeart not found on VPOperateManager.");
        });

    public Task StartLiveSpo2Async(CancellationToken cancellationToken = default) =>
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureSession();
            var manager = _manager!;
            var write = CreateWriteResponseProxy();
            var spo2Proxy = CreateProxy(
                "com.veepoo.protocol.listener.data.ISpo2hDataListener",
                (method, args) =>
                {
                    if ((method.Name is not ("onSpO2HADataChange" or "onDataChange")) || args.Length == 0 || args[0] is null)
                        return null;

                    try
                    {
                        var value = ReadIntProperty(args[0]!, "getValue", "value");
                        var pulse = ReadIntProperty(args[0]!, "getRateValue", "rateValue");
                        if (value is >= 50 and <= 100)
                        {
                            RaiseVitals(new WearableVitalsSnapshot
                            {
                                At = DateTimeOffset.UtcNow,
                                SpO2Percent = value,
                                SpO2PulseBpm = pulse is > 0 ? pulse : null,
                                HeartRateBpm = pulse is >= 20 and <= 300 ? pulse : null,
                                CharacteristicUuid = "hband:spo2",
                                RawHex = $"spo2={value}"
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warn(Tag, "SpO2 parse failed: " + ex.Message);
                    }

                    return null;
                });

            var lightProxy = CreateProxy(
                "com.veepoo.protocol.listener.data.ILightDataCallBack",
                (_, _) => null);

            if (!TryInvoke(manager, "startDetectSPO2H", write, spo2Proxy, lightProxy)
                && !TryInvoke(manager, "startDetectSPO2H", write, spo2Proxy))
            {
                throw new InvalidOperationException("startDetectSPO2H not found on VPOperateManager.");
            }
        });

    public Task DisconnectAsync(CancellationToken cancellationToken = default) =>
        MainThread.InvokeOnMainThreadAsync(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            await DisconnectCoreAsync().ConfigureAwait(true);
        });

    public void Dispose()
    {
        try
        {
            DisconnectCoreAsync().GetAwaiter().GetResult();
        }
        catch
        {
            // best-effort
        }
    }

    private async Task ConfirmPasswordAsync(Java.Lang.Object manager, string password, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var reg = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));

        var write = CreateWriteResponseProxy();
        var pwdProxy = CreateProxy(
            "com.veepoo.protocol.listener.data.IPwdDataListener",
            (method, _) =>
            {
                if (method.Name == "onPwdDataChange")
                    tcs.TrySetResult(true);
                return null;
            });
        var functionProxy = CreateProxy("com.veepoo.protocol.listener.data.IDeviceFuctionDataListener", (_, _) => null);
        var socialProxy = CreateProxy("com.veepoo.protocol.listener.data.ISocialMsgDataListener", (_, _) => null);
        var customProxy = CreateProxy("com.veepoo.protocol.listener.data.ICustomSettingDataListener", (_, _) => null);

        var pwd = new Java.Lang.String(string.IsNullOrWhiteSpace(password) ? HBandSdkInfo.DefaultDevicePasswordPlaceholder : password);
        var is24 = Java.Lang.Boolean.ValueOf(true);

        var ok = TryInvoke(manager, "confirmDevicePwd", write, pwdProxy, functionProxy, socialProxy, customProxy, pwd, is24)
                 || TryInvoke(manager, "confirmDevicePwd", write, pwdProxy, functionProxy, socialProxy, pwd, is24);
        if (!ok)
            throw new InvalidOperationException("confirmDevicePwd overload not found.");

        // Some firmwares never fire pwd callback if already paired; allow timeout fall-through.
        var completed = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(20), cancellationToken)).ConfigureAwait(true);
        if (completed != tcs.Task)
            Log.Warn(Tag, "confirmDevicePwd timed out; continuing to syncPersonInfo.");
        else
            await tcs.Task.ConfigureAwait(true);
    }

    private async Task SyncPersonInfoAsync(Java.Lang.Object manager, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var reg = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));

        var write = CreateWriteResponseProxy();
        var personListener = CreateProxy(
            "com.veepoo.protocol.listener.data.IPersonInfoDataListener",
            (method, _) =>
            {
                if (method.Name.Contains("Person", StringComparison.OrdinalIgnoreCase))
                    tcs.TrySetResult(true);
                return null;
            });

        var personData = CreateDefaultPersonInfo();
        if (personData is null || !TryInvoke(manager, "syncPersonInfo", write, personListener, personData))
            throw new InvalidOperationException("syncPersonInfo failed to invoke.");

        var completed = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(15), cancellationToken)).ConfigureAwait(true);
        if (completed != tcs.Task)
            Log.Warn(Tag, "syncPersonInfo timed out; session may still work for detect calls.");
        else
            await tcs.Task.ConfigureAwait(true);
    }

    private Task DisconnectCoreAsync()
    {
        lock (_sync)
        {
            _sessionReady = false;
            _connectedMac = null;
        }

        if (_manager is null)
            return Task.CompletedTask;

        try
        {
            TryInvoke(_manager, "stopDetectHeart", CreateWriteResponseProxy());
            TryInvoke(_manager, "stopDetectSPO2H", CreateWriteResponseProxy());
            var write = CreateWriteResponseProxy();
            if (!TryInvoke(_manager, "disconnectWatch", write))
                TryInvoke(_manager, "disconnect", write);
        }
        catch (Exception ex)
        {
            Log.Warn(Tag, "Disconnect: " + ex.Message);
            RaiseError(ex.Message);
        }

        return Task.CompletedTask;
    }

    private void EnsureInitialized()
    {
        if (_initialized && _manager is not null)
            return;

        var mgrClass = Class.ForName("com.veepoo.protocol.VPOperateManager");
        Java.Lang.Object? manager = null;
        foreach (var name in new[] { "getInstance", "getMangerInstance" })
        {
            try
            {
                var m = mgrClass.GetMethod(name, Class.FromType(typeof(global::Android.Content.Context)));
                manager = m.Invoke(null, global::Android.App.Application.Context) as Java.Lang.Object;
                if (manager is not null)
                    break;
            }
            catch (Throwable)
            {
                // try next
            }

            try
            {
                var m = mgrClass.GetMethod(name);
                manager = m.Invoke(null) as Java.Lang.Object;
                if (manager is not null)
                    break;
            }
            catch (Throwable)
            {
                // try next
            }
        }

        if (manager is null)
            throw new InvalidOperationException("Could not obtain VPOperateManager instance.");

        TryInvoke(manager, "init", global::Android.App.Application.Context);
        _manager = manager;
        _initialized = true;
    }

    private void EnsureSession()
    {
        EnsureInitialized();
        if (!IsSessionReady)
            throw new InvalidOperationException("HBand session is not ready. Connect and handshake first.");
    }

    private Java.Lang.Object CreateWriteResponseProxy() =>
        CreateProxy(
            "com.veepoo.protocol.listener.base.IBleWriteResponse",
            (method, args) =>
            {
                if (method.Name == "onResponse" && args.Length > 0)
                {
                    var code = UnboxInt(args[0]);
                    if (code != RequestSuccessCode())
                        Log.Debug(Tag, $"BleWriteResponse code={code}");
                }

                return null;
            },
            fallbackInterfaceNames:
            [
                "com.inuker.bluetooth.library.connect.response.BleWriteResponse",
                "com.veepoo.protocol.listener.base.BleWriteResponse"
            ]);

    private Java.Lang.Object CreateProxy(
        string primaryInterface,
        Func<Method, Java.Lang.Object?[], Java.Lang.Object?> handler,
        string[]? fallbackInterfaceNames = null)
    {
        Class? iface = null;
        foreach (var name in new[] { primaryInterface }.Concat(fallbackInterfaceNames ?? []))
        {
            try
            {
                iface = Class.ForName(name);
                break;
            }
            catch (Throwable)
            {
                // try next
            }
        }

        if (iface is null)
            throw new InvalidOperationException($"Interface not found: {primaryInterface}");

        var loader = iface.ClassLoader ?? throw new InvalidOperationException("Missing class loader.");
        _activeHandler = new HBandInvocationHandler(handler);
        return Proxy.NewProxyInstance(loader, [iface], _activeHandler)
               ?? throw new InvalidOperationException("Proxy.NewProxyInstance returned null.");
    }

    private static Java.Lang.Object? CreateDefaultPersonInfo()
    {
        try
        {
            var sexClass = Class.ForName("com.veepoo.protocol.model.enums.ESex");
            var sexObj = sexClass.GetField("MAN")?.Get(null)
                         ?? sexClass.GetField("MALE")?.Get(null)
                         ?? sexClass.GetEnumConstants()?[0];
            if (sexObj is null)
                return null;

            var personClass = Class.ForName("com.veepoo.protocol.model.datas.PersonInfoData");
            foreach (var ctor in personClass.GetConstructors())
            {
                var pts = ctor.GetParameterTypes() ?? [];
                if (pts.Length == 5)
                {
                    return ctor.NewInstance(
                        sexObj,
                        Integer.ValueOf(170),
                        Integer.ValueOf(65),
                        Integer.ValueOf(30),
                        Integer.ValueOf(8000));
                }

                if (pts.Length == 6)
                {
                    return ctor.NewInstance(
                        sexObj,
                        Integer.ValueOf(170),
                        Integer.ValueOf(65),
                        Integer.ValueOf(30),
                        Integer.ValueOf(8000),
                        Integer.ValueOf(480));
                }
            }
        }
        catch (Throwable t)
        {
            Log.Error(Tag, "PersonInfoData: " + t);
        }

        return null;
    }

    private static bool TryInvoke(Java.Lang.Object target, string methodName, params Java.Lang.Object?[] args)
    {
        try
        {
            foreach (var method in target.Class.GetMethods())
            {
                if (method.Name != methodName)
                    continue;
                var pts = method.GetParameterTypes() ?? [];
                if (pts.Length != args.Length)
                    continue;
                method.Invoke(target, args!);
                return true;
            }
        }
        catch (Throwable t)
        {
            Log.Warn(Tag, $"{methodName} invoke failed: {t.Message}");
        }

        return false;
    }

    private static int RequestSuccessCode()
    {
        try
        {
            var codeClass = Class.ForName("com.inuker.bluetooth.library.Code");
            var field = codeClass.GetField("REQUEST_SUCCESS");
            return UnboxInt(field?.Get(null));
        }
        catch
        {
            return 0;
        }
    }

    private static int UnboxInt(Java.Lang.Object? value)
    {
        if (value is null)
            return -1;
        if (value is Integer i)
            return i.IntValue();
        if (value is Java.Lang.Boolean b)
            return b.BooleanValue() ? 0 : -1;
        try
        {
            return Integer.ParseInt(value.ToString());
        }
        catch
        {
            return -1;
        }
    }

    private static int? ReadIntProperty(Java.Lang.Object javaObj, params string[] names)
    {
        var cls = javaObj.Class;
        foreach (var name in names)
        {
            try
            {
                var m = cls.GetMethod(name);
                return UnboxInt(m.Invoke(javaObj));
            }
            catch
            {
                // try field
            }

            try
            {
                var f = cls.GetField(name);
                return UnboxInt(f.Get(javaObj));
            }
            catch
            {
                // next
            }
        }

        return null;
    }

    private void RaiseVitals(WearableVitalsSnapshot snap) =>
        MainThread.BeginInvokeOnMainThread(() => VitalsUpdated?.Invoke(this, snap));

    private sealed class HBandInvocationHandler(Func<Method, Java.Lang.Object?[], Java.Lang.Object?> handler)
        : Java.Lang.Object, IInvocationHandler
    {
        public Java.Lang.Object? Invoke(Java.Lang.Object? proxy, Method? method, Java.Lang.Object[]? args)
        {
            if (method is null)
                return null;
            if (method.Name is "toString" or "hashCode" or "equals")
                return method.Invoke(this, args);

            return handler(method, args ?? []);
        }
    }
}
