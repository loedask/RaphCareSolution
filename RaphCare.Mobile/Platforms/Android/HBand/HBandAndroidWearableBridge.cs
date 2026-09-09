using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Android.Util;
using Java.Lang;
using Java.Lang.Reflect;
using Microsoft.Maui.ApplicationModel;
using RaphCare.Mobile.Core.Features.Devices.HBand;
using RaphCare.Mobile.Core.Features.Devices.Models;
using RaphCare.Mobile.Kernel.Core.Common.Devices;
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
    private bool _vendorConnectStarted;
    private bool _heartDetectStarted;
    private bool _spo2DetectStarted;
    private readonly List<Java.Lang.Object> _proxyRoots = new();
    private readonly List<HBandInvocationHandler> _handlerRoots = new();
    private HBandInvocationHandler? _activeHandler;

    private bool? _sdkAvailableCached;

    public bool IsAvailable
    {
        get
        {
            if (_sdkAvailableCached is bool cached)
                return cached;

            try
            {
                LoadSdkClass("com.veepoo.protocol.VPOperateManager");
                _sdkAvailableCached = true;
                return true;
            }
            catch (Throwable t)
            {
                Log.Warn(Tag, "VPOperateManager missing (watch SDK not in this build): " + t.Message);
                _sdkAvailableCached = false;
                return false;
            }
            catch (Exception ex)
            {
                Log.Warn(Tag, "VPOperateManager missing (watch SDK not in this build): " + ex.Message);
                _sdkAvailableCached = false;
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

    public async Task ConnectAndHandshakeAsync(
        string macAddress,
        string? deviceName,
        string devicePassword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(macAddress))
            throw new ArgumentException("MAC address is required.", nameof(macAddress));

        cancellationToken.ThrowIfCancellationRequested();
        // Only hop to the UI thread for short JNI calls. Awaiting connect/notify callbacks
        // while occupying the main looper deadlocks Inuker (ANR / process kill on Measure).
        await MainThread.InvokeOnMainThreadAsync(EnsureInitialized).ConfigureAwait(false);
        var manager = _manager ?? throw new InvalidOperationException("VPOperateManager is null.");

        lock (_sync)
        {
            if (_sessionReady
                && BluetoothMacNormalizer.EqualsNormalized(_connectedMac, macAddress))
                return;
        }

        Exception? lastError = null;
        for (var attempt = 1; attempt <= DevicesBleSessionPolicy.VendorConnectMaxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var connectTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                var notifyTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                linked.CancelAfter(TimeSpan.FromSeconds(40));
                using var reg = linked.Token.Register(() =>
                {
                    connectTcs.TrySetCanceled(linked.Token);
                    notifyTcs.TrySetCanceled(linked.Token);
                });

                // If Veepoo already holds this MAC (common after a soft Disconnect or a hung
                // prior attempt), skip connectDevice. Calling it again often never callbacks.
                Log.Info(Tag, $"CONNECT-1 native check: {macAddress}");
                if (await TryAdoptExistingNativeLinkAsync(manager, macAddress, cancellationToken)
                        .ConfigureAwait(false))
                {
                    Log.Info(Tag, $"CONNECT-1 adopted existing link: {macAddress}");
                    await ConfirmPasswordAsync(manager, devicePassword, cancellationToken)
                        .ConfigureAwait(false);
                    await SyncPersonInfoAsync(manager, cancellationToken).ConfigureAwait(false);
                    lock (_sync)
                    {
                        _connectedMac = macAddress;
                        _sessionReady = true;
                        _vendorConnectStarted = true;
                    }

                    return;
                }

                // Stale connectStarted / half-open radio: tear down before a fresh connectDevice.
                var mayTeardown = DevicesBleSessionPolicy.ShouldInvokeVendorDisconnect(
                    IsSessionReady,
                    Volatile.Read(ref _vendorConnectStarted));
                if (mayTeardown || IsAnyNativeConnected(manager))
                {
                    await MainThread.InvokeOnMainThreadAsync(() => DisconnectCoreAsync())
                        .ConfigureAwait(false);
                    await Task.Delay(TimeSpan.FromMilliseconds(1200 * attempt), cancellationToken)
                        .ConfigureAwait(false);
                }
                else if (attempt > 1)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(900 * attempt), cancellationToken)
                        .ConfigureAwait(false);
                }

                // Empty device name has crashed reconnect after Disconnect on some firmware.
                var safeName = string.IsNullOrWhiteSpace(deviceName) ? "ET580" : deviceName.Trim();
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    try
                    {
                        var connectProxy = CreateProxy(
                            "com.veepoo.protocol.listener.base.IConnectResponse",
                            (method, args) =>
                            {
                                Log.Debug(Tag, $"IConnectResponse.{method.Name} args={args.Length}");
                                if (method.Name is "connectState" or "onResponse" or "onConnectResponse")
                                {
                                    var code = UnboxInt(args.Length > 0 ? args[0] : null);
                                    Log.Debug(Tag, $"connectState code={code}");
                                    if (code == RequestSuccessCode())
                                        connectTcs.TrySetResult(true);
                                    else
                                        connectTcs.TrySetException(
                                            new InvalidOperationException(FormatConnectFailure(code)));
                                }

                                return null;
                            });

                        var notifyProxy = CreateProxy(
                            "com.veepoo.protocol.listener.base.INotifyResponse",
                            (method, args) =>
                            {
                                Log.Debug(Tag, $"INotifyResponse.{method.Name} args={args.Length}");
                                if (method.Name is "notifyState" or "onResponse" or "notifySuccess" or "onNotifyResponse")
                                {
                                    var code = args.Length > 0 ? UnboxInt(args[0]) : RequestSuccessCode();
                                    Log.Debug(Tag, $"notifyState code={code}");
                                    if (code == RequestSuccessCode() || method.Name is "notifySuccess")
                                        notifyTcs.TrySetResult(true);
                                    else
                                        notifyTcs.TrySetException(
                                            new InvalidOperationException($"HBand notify failed (code {code})."));
                                }

                                return null;
                            });

                        var mac = new Java.Lang.String(macAddress);
                        var name = new Java.Lang.String(safeName);

                        // Prefer mac+name, then mac-only. Typed TryInvoke avoids wrong overloads.
                        // If logcat ends at CONNECT-2 with no CONNECT-3, connectDevice aborted the process.
                        Log.Info(Tag, $"CONNECT-2 calling connectDevice: {macAddress}, {safeName}");
                        var connected = TryInvoke(manager, "connectDevice", mac, name, connectProxy, notifyProxy)
                                        || TryInvoke(manager, "connectDevice", mac, connectProxy, notifyProxy);
                        Log.Info(Tag, $"CONNECT-3 connectDevice returned: {connected}");
                        if (!connected)
                        {
                            throw new InvalidOperationException(
                                "connectDevice overload not found on VPOperateManager.");
                        }

                        Volatile.Write(ref _vendorConnectStarted, true);
                    }
                    catch (Throwable t)
                    {
                        Log.Error(Tag, "connectDevice threw: " + t);
                        connectTcs.TrySetException(
                            new InvalidOperationException(
                                "Watch Bluetooth could not start. Keep the watch nearby and unlocked, then Connect again."));
                    }
                }).ConfigureAwait(false);

                try
                {
                    // Wait off the main thread so connectState can be delivered on the looper.
                    await connectTcs.Task.ConfigureAwait(false);
                    await notifyTcs.Task.ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    await MainThread.InvokeOnMainThreadAsync(() => DisconnectCoreAsync())
                        .ConfigureAwait(false);
                    throw new TimeoutException(
                        "Watch connect timed out. Keep the watch nearby and unlocked, force-stop the H Band app if it is installed, then Connect again.");
                }

                await ConfirmPasswordAsync(manager, devicePassword, cancellationToken).ConfigureAwait(false);
                await SyncPersonInfoAsync(manager, cancellationToken).ConfigureAwait(false);

                lock (_sync)
                {
                    _connectedMac = macAddress;
                    _sessionReady = true;
                }

                return;
            }
            catch (TimeoutException ex) when (attempt < DevicesBleSessionPolicy.VendorConnectMaxAttempts)
            {
                lastError = ex;
                Log.Warn(Tag, $"Connect attempt {attempt} timed out; clearing radio and retrying.");
            }
            catch (InvalidOperationException ex) when (
                attempt < DevicesBleSessionPolicy.VendorConnectMaxAttempts
                && HBandConnectFailureMessages.IsRadioBusyCancel(ex.Message))
            {
                lastError = ex;
                Log.Warn(Tag, $"Connect attempt {attempt} canceled (-2); retrying.");
            }
        }

        throw lastError
              ?? new InvalidOperationException(
                  HBandConnectFailureMessages.ForCode(HBandConnectFailureMessages.RequestCanceled));
    }

    private static string FormatConnectFailure(int code) =>
        HBandConnectFailureMessages.ForCode(code);


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
                    Log.Debug(Tag, $"IHeartDataListener.{method.Name} args={args.Length}");
                    if (method.Name is not ("onDataChange" or "onHeartDataChange" or "heartDataChange")
                        || args.Length == 0
                        || args[0] is null)
                        return null;

                    try
                    {
                        var heart = args[0]!;
                        var statusName = ReadEnumName(heart, "getHeartStatus", "heartStatus");
                        var bpm = ReadIntProperty(heart, "getData", "data", "getHeartRate", "heartRate", "getValue", "value");
                        Log.Debug(Tag, $"Heart sample status={statusName ?? "(null)"} bpm={bpm?.ToString(CultureInfo.InvariantCulture) ?? "(null)"} raw={heart}");

                        if (WearableHeartDetectRules.IsBlockingHeartStatus(statusName))
                        {
                            var msg = WearableHeartDetectRules.PatientMessageForHeartStatus(statusName);
                            if (!string.IsNullOrWhiteSpace(msg))
                                RaiseError(msg);
                            return null;
                        }

                        if (WearableHeartDetectRules.ShouldAcceptHeartSample(statusName, bpm))
                        {
                            RaiseVitals(new WearableVitalsSnapshot
                            {
                                At = DateTimeOffset.UtcNow,
                                HeartRateBpm = bpm,
                                CharacteristicUuid = "hband:heart",
                                RawHex = $"hr={bpm};status={statusName}"
                            });
                        }
                        else
                        {
                            Log.Warn(Tag, $"Heart callback ignored status={statusName} bpm={bpm}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warn(Tag, "Heart parse failed: " + ex.Message);
                    }

                    return null;
                });

            try
            {
                if (!TryInvoke(manager, "startDetectHeart", write, heartProxy)
                    && !TryInvoke(manager, "startDetectHeart", heartProxy))
                    throw new InvalidOperationException("startDetectHeart not found on VPOperateManager.");

                lock (_sync)
                    _heartDetectStarted = true;
            }
            catch (Throwable t)
            {
                Log.Error(Tag, "startDetectHeart threw: " + t);
                throw new InvalidOperationException("Watch heart measure failed to start.", t);
            }
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

            lock (_sync)
                _spo2DetectStarted = true;
        });

    public Task StopLiveDetectionsAsync(CancellationToken cancellationToken = default) =>
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_manager is null)
                return;

            bool stopHeart;
            bool stopSpo2;
            lock (_sync)
            {
                stopHeart = DevicesBleSessionPolicy.ShouldInvokeVendorStopDetect(_heartDetectStarted);
                stopSpo2 = DevicesBleSessionPolicy.ShouldInvokeVendorStopDetect(_spo2DetectStarted);
            }

            if (!stopHeart && !stopSpo2)
                return;

            try
            {
                if (stopHeart)
                    TryInvoke(_manager, "stopDetectHeart", CreateWriteResponseProxy());
                if (stopSpo2)
                    TryInvoke(_manager, "stopDetectSPO2H", CreateWriteResponseProxy());
            }
            catch (Throwable t)
            {
                Log.Warn(Tag, "StopLiveDetections: " + t.Message);
            }
            catch (Exception ex)
            {
                Log.Warn(Tag, "StopLiveDetections: " + ex.Message);
            }
            finally
            {
                lock (_sync)
                {
                    if (stopHeart)
                        _heartDetectStarted = false;
                    if (stopSpo2)
                        _spo2DetectStarted = false;
                }
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
        // Do not block the calling thread (often UI) on disconnect; that can ANR during Measure.
        _ = MainThread.InvokeOnMainThreadAsync(() =>
        {
            try
            {
                DisconnectCoreAsync();
            }
            catch
            {
                // best-effort
            }
        });
    }

    private async Task ConfirmPasswordAsync(Java.Lang.Object manager, string password, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var reg = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
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
            // Official sample uses false for 24-hour model during pwd confirm.
            var is24 = Java.Lang.Boolean.ValueOf(false);

            var ok = TryInvoke(manager, "confirmDevicePwd", write, pwdProxy, functionProxy, socialProxy, customProxy, pwd, is24)
                     || TryInvoke(manager, "confirmDevicePwd", write, pwdProxy, functionProxy, socialProxy, pwd, is24);
            if (!ok)
                throw new InvalidOperationException("confirmDevicePwd overload not found.");
        }).ConfigureAwait(false);

        // Some firmwares never fire pwd callback if already paired; allow timeout fall-through.
        var completed = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(20), cancellationToken))
            .ConfigureAwait(false);
        if (completed != tcs.Task)
            Log.Warn(Tag, "confirmDevicePwd timed out; continuing to syncPersonInfo.");
        else
            await tcs.Task.ConfigureAwait(false);
    }

    private async Task SyncPersonInfoAsync(Java.Lang.Object manager, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var reg = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
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
        }).ConfigureAwait(false);

        var completed = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(15), cancellationToken))
            .ConfigureAwait(false);
        if (completed != tcs.Task)
            Log.Warn(Tag, "syncPersonInfo timed out; session may still work for detect calls.");
        else
            await tcs.Task.ConfigureAwait(false);
    }

    private Task DisconnectCoreAsync()
    {
        bool mayTeardown;
        bool stopHeart;
        bool stopSpo2;
        lock (_sync)
        {
            mayTeardown = DevicesBleSessionPolicy.ShouldInvokeVendorDisconnect(
                _sessionReady,
                _vendorConnectStarted);
            stopHeart = DevicesBleSessionPolicy.ShouldInvokeVendorStopDetect(_heartDetectStarted);
            stopSpo2 = DevicesBleSessionPolicy.ShouldInvokeVendorStopDetect(_spo2DetectStarted);
            _sessionReady = false;
            _connectedMac = null;
            _vendorConnectStarted = false;
            _heartDetectStarted = false;
            _spo2DetectStarted = false;
        }

        if (!mayTeardown || _manager is null)
            return Task.CompletedTask;

        try
        {
            if (stopHeart)
                TryInvoke(_manager, "stopDetectHeart", CreateWriteResponseProxy());
            if (stopSpo2)
                TryInvoke(_manager, "stopDetectSPO2H", CreateWriteResponseProxy());
            var write = CreateWriteResponseProxy();
            if (!TryInvoke(_manager, "disconnectWatch", write))
                TryInvoke(_manager, "disconnect", write);
        }
        catch (Throwable t)
        {
            Log.Warn(Tag, "Disconnect: " + t.Message);
        }
        catch (Exception ex)
        {
            Log.Warn(Tag, "Disconnect: " + ex.Message);
        }

        return Task.CompletedTask;
    }

    private void EnsureInitialized()
    {
        if (_initialized && _manager is not null)
            return;

        var mgrClass = LoadSdkClass("com.veepoo.protocol.VPOperateManager");
        Java.Lang.Object? manager = null;
        var appContext = global::Android.App.Application.Context;
        foreach (var name in new[] { "getMangerInstance", "getInstance" })
        {
            try
            {
                var m = mgrClass.GetMethod(name, Class.FromType(typeof(global::Android.Content.Context)));
                manager = m.Invoke(null, appContext) as Java.Lang.Object;
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

        TryInvoke(manager, "init", appContext);
        TryInvoke(manager, "setAutoConnectBTBySdk", Java.Lang.Boolean.ValueOf(false));
        _manager = manager;
        _initialized = true;
    }

    private static Task<bool> TryAdoptExistingNativeLinkAsync(
        Java.Lang.Object manager,
        string macAddress,
        CancellationToken cancellationToken) =>
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (IsDeviceConnected(manager, macAddress) || IsCurrentDeviceConnected(manager))
            {
                Log.Info(Tag, "Adopting existing Veepoo link for " + macAddress);
                return true;
            }

            return false;
        });

    private static bool IsAnyNativeConnected(Java.Lang.Object manager) =>
        IsCurrentDeviceConnected(manager);

    private static bool IsCurrentDeviceConnected(Java.Lang.Object manager)
    {
        try
        {
            foreach (var method in manager.Class.GetMethods())
            {
                if (method.Name != "isCurrentDeviceConnected")
                    continue;
                if ((method.GetParameterTypes()?.Length ?? -1) != 0)
                    continue;
                var result = method.Invoke(manager);
                return result is Java.Lang.Boolean b && b.BooleanValue();
            }
        }
        catch (Throwable t)
        {
            Log.Warn(Tag, "isCurrentDeviceConnected: " + t.Message);
        }

        return false;
    }

    private static bool IsDeviceConnected(Java.Lang.Object manager, string macAddress)
    {
        try
        {
            var mac = new Java.Lang.String(macAddress);
            foreach (var method in manager.Class.GetMethods())
            {
                if (method.Name != "isDeviceConnected")
                    continue;
                var pts = method.GetParameterTypes() ?? [];
                if (pts.Length != 1)
                    continue;
                var result = method.Invoke(manager, mac);
                if (result is Java.Lang.Boolean b && b.BooleanValue())
                    return true;
            }
        }
        catch (Throwable t)
        {
            Log.Warn(Tag, "isDeviceConnected: " + t.Message);
        }

        return false;
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
                iface = LoadSdkClass(name);
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
        var invocationHandler = new HBandInvocationHandler(handler);
        _activeHandler = invocationHandler;
        _handlerRoots.Add(invocationHandler);
        var proxy = Proxy.NewProxyInstance(loader, [iface], invocationHandler)
               ?? throw new InvalidOperationException("Proxy.NewProxyInstance returned null.");
        // Keep C# roots so the JNI proxy listener is not collected mid-callback.
        // Do not trim aggressively: dropping a live connect/notify proxy GC-collects it and
        // native Veepoo callbacks then crash the process.
        _proxyRoots.Add(proxy);
        return proxy;
    }

    private static Java.Lang.Object? CreateDefaultPersonInfo()
    {
        try
        {
            var sexClass = LoadSdkClass("com.veepoo.protocol.model.enums.ESex");
            var sexObj = sexClass.GetField("MAN")?.Get(null)
                         ?? sexClass.GetField("MALE")?.Get(null)
                         ?? sexClass.GetEnumConstants()?[0];
            if (sexObj is null)
                return null;

            var personClass = LoadSdkClass("com.veepoo.protocol.model.datas.PersonInfoData");
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
            var candidates = new List<(Method Method, int Score)>();
            foreach (var method in target.Class.GetMethods())
            {
                if (method.Name != methodName)
                    continue;
                if (!ParametersCompatible(method.GetParameterTypes() ?? [], args))
                    continue;
                candidates.Add((method, ScoreParameterMatch(method.GetParameterTypes() ?? [], args)));
            }

            foreach (var (method, _) in candidates.OrderByDescending(c => c.Score))
            {
                try
                {
                    method.Invoke(target, args!);
                    return true;
                }
                catch (Throwable t)
                {
                    Log.Warn(Tag, $"{methodName} invoke failed: {t.Message}");
                }
            }
        }
        catch (Throwable t)
        {
            Log.Warn(Tag, $"{methodName} invoke failed: {t.Message}");
        }

        return false;
    }

    private static bool ParametersCompatible(Class[] parameterTypes, Java.Lang.Object?[] args)
    {
        if (parameterTypes.Length != args.Length)
            return false;

        for (var i = 0; i < parameterTypes.Length; i++)
        {
            var arg = args[i];
            var pt = parameterTypes[i];
            if (arg is null)
            {
                if (pt.IsPrimitive)
                    return false;
                continue;
            }

            if (!pt.IsInstance(arg))
                return false;
        }

        return true;
    }

    private static int ScoreParameterMatch(Class[] parameterTypes, Java.Lang.Object?[] args)
    {
        var score = 0;
        for (var i = 0; i < parameterTypes.Length; i++)
        {
            if (args[i] is null)
                continue;
            var ptName = parameterTypes[i].Name ?? string.Empty;
            var argName = args[i]!.Class.Name ?? string.Empty;
            if (string.Equals(ptName, argName, StringComparison.Ordinal))
                score += 3;
            else if (ptName.Contains("Listener", StringComparison.Ordinal)
                     || ptName.Contains("Response", StringComparison.Ordinal))
                score += 1;
        }

        return score;
    }

    private static int RequestSuccessCode()
    {
        try
        {
            var codeClass = LoadSdkClass("com.inuker.bluetooth.library.Code");
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

    private static Class LoadSdkClass(string name)
    {
        // Multidex / Bind=false AAR classes are visible on the app Context classloader.
        // Class.ForName(name) alone can miss them and report "Watch SDK is unavailable".
        var loader = global::Android.App.Application.Context?.ClassLoader;
        if (loader is not null)
            return Class.ForName(name, initialize: true, loader);

        return Class.ForName(name);
    }

    private static string? ReadEnumName(Java.Lang.Object javaObj, params string[] names)
    {
        var cls = javaObj.Class;
        foreach (var name in names)
        {
            try
            {
                var m = cls.GetMethod(name);
                var value = m.Invoke(javaObj);
                if (value is null)
                    continue;
                if (value is Java.Lang.Enum e)
                    return e.Name();
                var nameMethod = value.Class.GetMethod("name");
                return nameMethod?.Invoke(value)?.ToString();
            }
            catch
            {
                // try next
            }

            try
            {
                var f = cls.GetField(name);
                var value = f.Get(javaObj);
                if (value is Java.Lang.Enum e)
                    return e.Name();
                return value?.ToString();
            }
            catch
            {
                // next
            }
        }

        return null;
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

    private void RaiseVitals(WearableVitalsSnapshot snap)
    {
        // Never do UI work on the binder/callback thread; schedule and return immediately.
        try
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    VitalsUpdated?.Invoke(this, snap);
                }
                catch (Exception ex)
                {
                    Log.Warn(Tag, "VitalsUpdated handler failed: " + ex.Message);
                }
            });
        }
        catch (Exception ex)
        {
            Log.Warn(Tag, "RaiseVitals schedule failed: " + ex.Message);
        }
    }

    private sealed class HBandInvocationHandler(Func<Method, Java.Lang.Object?[], Java.Lang.Object?> handler)
        : Java.Lang.Object, IInvocationHandler
    {
        public Java.Lang.Object? Invoke(Java.Lang.Object? proxy, Method? method, Java.Lang.Object[]? args)
        {
            if (method is null)
                return null;
            if (method.Name is "toString" or "hashCode" or "equals")
                return method.Invoke(this, args);

            try
            {
                var result = handler(method, args ?? []);
                return result ?? DefaultReturnValue(method);
            }
            catch (Throwable t)
            {
                // Veepoo / Inuker NPEs on the binder or UI proxy must not tear down MAUI Shell.
                Log.Error(Tag, "HBand proxy invoke failed: " + t);
                return DefaultReturnValue(method);
            }
            catch (Exception ex)
            {
                Log.Error(Tag, "HBand proxy invoke failed: " + ex.Message);
                return DefaultReturnValue(method);
            }
        }

        private static Java.Lang.Object? DefaultReturnValue(Method method)
        {
            // Returning null for non-void Java methods from a Proxy crashes the process.
            var returnType = method.ReturnType;
            if (returnType is null || returnType.Name is "void" or "V")
                return null;
            if (returnType == Class.FromType(typeof(bool)) || returnType.Name == "boolean")
                return Java.Lang.Boolean.ValueOf(false);
            if (returnType == Class.FromType(typeof(int)) || returnType.Name == "int")
                return Integer.ValueOf(0);
            if (returnType == Class.FromType(typeof(long)) || returnType.Name == "long")
                return Java.Lang.Long.ValueOf(0L);
            if (returnType == Class.FromType(typeof(float)) || returnType.Name == "float")
                return Java.Lang.Float.ValueOf(0f);
            if (returnType == Class.FromType(typeof(double)) || returnType.Name == "double")
                return Java.Lang.Double.ValueOf(0d);
            return null;
        }
    }
}
