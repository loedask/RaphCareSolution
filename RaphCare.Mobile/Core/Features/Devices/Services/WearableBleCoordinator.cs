using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using RaphCare.Mobile.Core.Features.Devices.HBand;
using RaphCare.Mobile.Core.Features.Devices.Models;
using RaphCare.Mobile.Kernel.Core.Common.Devices;
using BleDeviceEventArgs = Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs;

namespace RaphCare.Mobile.Core.Features.Devices.Services;

/// <summary>
/// BLE scan/connect via Plugin.BLE. Optional Veepoo/HBand path is gated by
/// <see cref="DevicesBleSessionPolicy.TryVendorSdkOnConnect"/> (off for patient builds to avoid vendor toasts).
/// </summary>
public sealed class WearableBleCoordinator : IWearableBleCoordinator, IDisposable
{
    private readonly IAdapter _adapter = CrossBluetoothLE.Current.Adapter;
    private readonly ConcurrentDictionary<Guid, IDevice> _devices = new();
    private readonly ConcurrentDictionary<Guid, string> _macById = new();
    private readonly List<(ICharacteristic Ch, EventHandler<CharacteristicUpdatedEventArgs> H)> _notifyHandlers = new();
    private readonly SemaphoreSlim _connectGate = new(1, 1);
    private readonly IHBandWearableBridge _hband;
    private readonly IVendorConnectStepProbe _connectStepProbe;

    private Guid? _connectedId;
    private bool _usingHbandSession;
    private string? _lastSessionMac;
    // The scan UI must retain this independently of Plugin.BLE discovery. A vendor MAC
    // session can be connectable even when the watch is not advertising as a peripheral.
    private string? _claimedWatchMac;
    private string? _claimedWatchDeviceName;
    private DateTimeOffset? _lastVendorDisconnectUtc;
    private string? _scanPreferredMac;
    private bool _scanShowAll;
    private WearableVitalsSnapshot? _lastVitals;
    private int _suppressDisconnectClear;

    /// <summary>Used when vendor reconnect succeeds before Plugin.BLE has rediscovered the peripheral.</summary>
    private static readonly Guid VendorSessionPlaceholderId = Guid.Parse("d0e5c001-b1e0-4a7c-9e58-000000000001");

    public WearableBleCoordinator(
        IHBandWearableBridge hband,
        IVendorConnectStepProbe connectStepProbe)
    {
        _hband = hband;
        _connectStepProbe = connectStepProbe ?? throw new ArgumentNullException(nameof(connectStepProbe));
        _hband.VitalsUpdated += OnHbandVitalsUpdated;
        _hband.ErrorOccurred += OnHbandError;
        _hband.ConnectStepChanged += OnHbandConnectStepChanged;

        _adapter.DeviceDisconnected += (_, e) =>
        {
            if (_suppressDisconnectClear > 0)
                return;
            // Exclusive Veepoo owns the radio; Plugin.BLE disconnect after handoff is expected.
            if (_usingHbandSession)
                return;
            if (_connectedId != e.Device.Id)
                return;
            _connectedId = null;
            _ = Task.Run(async () => await CleanupNotificationsAsync().ConfigureAwait(false));
        };
    }

    public bool IsBleSupported => BlePlatform.IsSupported;

    public bool IsScanning => _adapter.IsScanning;

    public Guid? ConnectedDeviceId => _connectedId;

    public bool IsVendorMeasureAvailable => _hband.IsAvailable;

    /// <summary>
    /// True when Measure can call startDetect on an exclusive vendor session
    /// (or when exclusive mode is off / vendor SDK missing).
    /// </summary>
    public bool IsLiveMeasureSessionReady =>
        DevicesBleSessionPolicy.IsExclusiveLiveMeasureSessionReady(
            DevicesBleSessionPolicy.PreferExclusiveVendorSession,
            _hband.IsAvailable,
            _usingHbandSession,
            _hband.IsSessionReady);

    public WearableVitalsSnapshot? LastVitals => _lastVitals;

    public IReadOnlyList<WearableDeviceDisplayItem> DiscoveredDevices
    {
        get
        {
            var devices = _devices.Values
            .Select(d => new WearableDeviceDisplayItem
            {
                Id = d.Id,
                Name = d.Name,
                Rssi = d.Rssi,
                MacAddress = _macById.GetValueOrDefault(d.Id)
            })
            .OrderByDescending(x => x.Rssi ?? int.MinValue)
            .ToList();

            var hasClaimedAdvertisement = devices.Any(d =>
                BluetoothMacNormalizer.EqualsNormalized(d.MacAddress, _claimedWatchMac));
            if (WearableBleScanRules.ShouldShowClaimedWatchFallback(
                    _claimedWatchMac,
                    hasClaimedAdvertisement))
            {
                devices.Add(new WearableDeviceDisplayItem
                {
                    Id = VendorSessionPlaceholderId,
                    Name = "Claimed wearable",
                    MacAddress = _claimedWatchMac,
                    Rssi = null,
                });
            }

            return devices;
        }
    }

    public event EventHandler? DiscoveredDevicesChanged;
    public event EventHandler<WearableVitalsSnapshot>? VitalsUpdated;
    public event EventHandler<string?>? ErrorOccurred;
    public event EventHandler<string>? VendorConnectStepChanged;

    public async Task<PermissionStatus> RequestBluetoothPermissionsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // Runtime permission dialogs must run on the UI thread. Off-thread RequestAsync can
        // return without granting Nearby devices, then Scan throws SecurityException.
        return await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var status = await Permissions.CheckStatusAsync<BluetoothPermissions>().ConfigureAwait(true);
            if (status == PermissionStatus.Granted)
                return status;

            var requested = await Permissions.RequestAsync<BluetoothPermissions>().ConfigureAwait(true);
            if (requested == PermissionStatus.Granted
                && DevicesBleSessionPolicy.PostPermissionGrantSettle > TimeSpan.Zero)
            {
                // Grant returns while the Activity is still finishing the permission result.
                // Starting Veepoo connectDevice in that window force-closes some phones.
                await Task.Delay(DevicesBleSessionPolicy.PostPermissionGrantSettle).ConfigureAwait(true);
            }

            return requested;
        }).ConfigureAwait(false);
    }

    public Task<PermissionStatus> CheckBluetoothPermissionsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return MainThread.InvokeOnMainThreadAsync(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await Permissions.CheckStatusAsync<BluetoothPermissions>().ConfigureAwait(true);
        });
    }

    public Task<bool> EnsureBluetoothAdapterOnAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!IsBleSupported)
            return Task.FromResult(false);
        return Task.FromResult(CrossBluetoothLE.Current.State == BluetoothState.On);
    }

    public async Task StartScanAsync(
        bool showAllDevices,
        string? preferredMacAddress = null,
        CancellationToken cancellationToken = default)
    {
        if (!IsBleSupported)
        {
            ErrorOccurred?.Invoke(this, "BLE is not supported on this platform build.");
            return;
        }

        if (CrossBluetoothLE.Current.State != BluetoothState.On)
        {
            ErrorOccurred?.Invoke(this, "Bluetooth is off. Turn it on and try again.");
            return;
        }

        await StopScanAsync().ConfigureAwait(false);

        // Plugin.BLE GATT can hide the peripheral from Scan. Release GATT only.
        // Keep exclusive Veepoo sessions alive (native disconnectWatch during Scan,
        // then Connect, force-closes some phones).
        var pluginConnected = _devices.Values.Any(d => d.State != DeviceState.Disconnected)
                              || (_connectedId is Guid cid
                                  && _devices.TryGetValue(cid, out var linked)
                                  && linked.State != DeviceState.Disconnected);
        if (WearableBleScanRules.ShouldReleaseActiveLinksBeforeManualScan(pluginConnected))
        {
            await ReleaseActiveLinksForManualScanAsync(cancellationToken).ConfigureAwait(false);
        }

        var preferredMac = BluetoothMacNormalizer.TryNormalize(preferredMacAddress)
                           ?? BluetoothMacNormalizer.TryNormalize(_lastSessionMac);
        _claimedWatchMac = preferredMac;
        _devices.Clear();
        _macById.Clear();
        // Raise after retaining the claimed MAC so Nearby immediately has a usable saved
        // row while Plugin.BLE performs its 30-second scan.
        RaiseDiscoveredChanged();
        SeedPairedOrConnectedDevices(preferredMac, showAllDevices);
        await ScanIntoCacheAsync(showAllDevices, preferredMac, 30_000, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task ReleaseActiveLinksForManualScanAsync(CancellationToken cancellationToken)
    {
        await CleanupNotificationsAsync().ConfigureAwait(false);

        // Never call _hband.DisconnectAsync here. Exclusive Veepoo stays up during Scan.
        foreach (var device in _devices.Values.ToList())
        {
            if (device.State == DeviceState.Disconnected)
                continue;
            await ReleasePluginBleLinkAsync(device, cancellationToken).ConfigureAwait(false);
        }

        // Clear Connected only when it was Plugin.BLE GATT, not a retained vendor session.
        if (!_usingHbandSession && !_hband.IsSessionReady)
            _connectedId = null;
    }

    private void SeedPairedOrConnectedDevices(string? preferredMac, bool showAllDevices)
    {
        try
        {
            IReadOnlyList<IDevice> known;
            try
            {
                known = _adapter.GetSystemConnectedOrPairedDevices();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetSystemConnectedOrPairedDevices: {ex.Message}");
                return;
            }

            var added = false;
            foreach (var device in known)
            {
                var mac = TryReadMacAddress(device);
                if (!WearableBleScanRules.IncludeInFilteredScan(
                        device.Name,
                        mac,
                        preferredMac,
                        showAllDevices))
                    continue;

                _devices[device.Id] = device;
                if (!string.IsNullOrWhiteSpace(mac))
                    _macById[device.Id] = mac!;
                added = true;
            }

            if (added)
                RaiseDiscoveredChanged();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Seed paired watches: {ex.Message}");
        }
    }

    private async Task ScanIntoCacheAsync(
        bool showAllDevices,
        string? preferredMac,
        int timeoutMs,
        CancellationToken cancellationToken)
    {
        _scanShowAll = showAllDevices;
        _scanPreferredMac = preferredMac;
        _adapter.ScanTimeout = timeoutMs;
        _adapter.DeviceDiscovered -= OnDeviceDiscovered;
        _adapter.DeviceDiscovered += OnDeviceDiscovered;

        using var stopOnCancel = cancellationToken.Register(() =>
        {
            try
            {
                if (_adapter.IsScanning)
                    _ = _adapter.StopScanningForDevicesAsync();
            }
            catch
            {
                // Best-effort stop when reconnect or the Devices page cancels the scan.
            }
        });

        try
        {
            // When a claimed MAC is known (or Show all is on), accept ads in Plugin.BLE and
            // filter in OnDeviceDiscovered. Early name/MAC filtering drops blank-name bands.
            var permissive = WearableBleScanRules.UsePermissivePluginBleScanFilter(
                showAllDevices,
                hasPreferredMac: preferredMac is not null);
            await _adapter.StartScanningForDevicesAsync(
                    new ScanFilterOptions(),
                    d => permissive || WearableBleScanRules.MatchesE580StyleName(d.Name),
                    false,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(
                this,
                BlePermissionFailure.IsMissingNearbyDevicesPermission(ex.Message)
                    ? "Nearby devices permission is required. Allow it in phone Settings for RaphCare, then try again."
                    : ex.Message);
        }
        finally
        {
            _adapter.DeviceDiscovered -= OnDeviceDiscovered;
            _scanPreferredMac = null;
            _scanShowAll = false;
        }
    }

    public async Task StopScanAsync()
    {
        if (!_adapter.IsScanning)
            return;
        try
        {
            await _adapter.StopScanningForDevicesAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex.Message);
        }
    }

    public async Task WarmUpVendorSdkAsync(CancellationToken cancellationToken = default)
    {
        if (!DevicesBleSessionPolicy.WarmUpVendorSdkOnDevicesAppear || !_hband.IsAvailable)
            return;

        try
        {
            await _hband.WarmUpAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Init probe must not block Devices; Connect still has its own init path.
            ErrorOccurred?.Invoke(this, "Watch SDK warm-up: " + ex.Message);
        }
    }

    public bool TryConsumeVendorConnectCrashMessage(out string patientMessage)
    {
        patientMessage = string.Empty;
        try
        {
            var step = _connectStepProbe.TryConsumeIncompleteStep();
            if (step is null)
                return false;

            patientMessage = VendorConnectCrashProbeRules.PatientMessageForIncompleteStep(step);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task ConnectViaVendorScanProbeAsync(
        string? preferredMacAddress,
        string? preferredDeviceName,
        CancellationToken cancellationToken = default)
    {
        if (!IsBleSupported)
            return;

        if (!VeepooSdkInitRules.ShouldUseVendorNativeScan(DevicesBleSessionPolicy.UseVeepooNativeScanProbe))
        {
            ErrorOccurred?.Invoke(this, "Vendor scan probe is off in this build.");
            return;
        }

        if (!_hband.IsAvailable)
        {
            ErrorOccurred?.Invoke(this,
                "Watch SDK is missing from this install. Reinstall the latest RaphCare APK.");
            return;
        }

        var preferredMac = BluetoothMacNormalizer.TryNormalize(preferredMacAddress);
        if (preferredMac is not null)
            _claimedWatchMac = preferredMac;
        if (!string.IsNullOrWhiteSpace(preferredDeviceName))
            _claimedWatchDeviceName = preferredDeviceName.Trim();

        await _connectGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var perm = await RequestBluetoothPermissionsAsync(cancellationToken).ConfigureAwait(false);
            if (perm != PermissionStatus.Granted)
            {
                ErrorOccurred?.Invoke(
                    this,
                    "Nearby devices permission is required. Allow it in phone Settings for RaphCare, then try again.");
                return;
            }

            if (!await EnsureBluetoothAdapterOnAsync(cancellationToken).ConfigureAwait(false))
            {
                ErrorOccurred?.Invoke(this, "Bluetooth is off. Turn it on and try again.");
                return;
            }

            // Free Plugin.BLE only if it already holds the radio. Do not StartScanning or
            // ConnectToDevice on Plugin.BLE for this probe session.
            await StopScanAsync().ConfigureAwait(false);
            await CleanupNotificationsAsync().ConfigureAwait(false);
            await ReleaseActivePluginBleWithoutVendorHandoffAsync(cancellationToken)
                .ConfigureAwait(false);

            if (DevicesBleSessionPolicy.ShouldReuseExistingVendorSession(
                    _hband.IsSessionReady,
                    _hband.ConnectedMacAddress,
                    preferredMac ?? _claimedWatchMac))
            {
                _usingHbandSession = true;
                _lastSessionMac = preferredMac ?? _claimedWatchMac;
                BindConnectedIdFromMac(_lastSessionMac!);
                ClearVendorDisconnectSettle();
                return;
            }

            var matchTcs = new TaskCompletionSource<(string Mac, string? Name)>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            void OnVendorDeviceFound(object? sender, VendorScanDeviceFoundEventArgs e)
            {
                if (!VeepooSdkInitRules.ShouldAcceptVendorScanResult(
                        e.Name,
                        e.MacAddress,
                        preferredMac,
                        WearableBleScanRules.MatchesE580StyleName(e.Name)))
                    return;

                matchTcs.TrySetResult((e.MacAddress, e.Name));
            }

            _hband.VendorScanDeviceFound += OnVendorDeviceFound;
            try
            {
                await _hband.StartVendorScanAsync(cancellationToken).ConfigureAwait(false);

                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(DevicesBleSessionPolicy.VendorNativeScanTimeout);
                var timedOut = Task.Delay(Timeout.InfiniteTimeSpan, timeoutCts.Token);
                var completed = await Task.WhenAny(matchTcs.Task, timedOut).ConfigureAwait(false);
                if (completed != matchTcs.Task)
                {
                    ErrorOccurred?.Invoke(this,
                        "Vendor scan did not find the watch in time. Keep it nearby and unlocked, then try again.");
                    return;
                }

                var (mac, name) = await matchTcs.Task.ConfigureAwait(false);
                await _hband.StopVendorScanAsync(cancellationToken).ConfigureAwait(false);

                await _hband.ConnectAndHandshakeAsync(
                        mac,
                        ResolveClaimedDeviceName(name ?? preferredDeviceName),
                        HBandSdkInfo.DefaultDevicePasswordPlaceholder,
                        cancellationToken)
                    .ConfigureAwait(false);

                _usingHbandSession = true;
                _lastSessionMac = mac;
                _claimedWatchMac = mac;
                BindConnectedIdFromMac(mac);
                ClearVendorDisconnectSettle();
            }
            catch (Exception ex)
            {
                _usingHbandSession = false;
                _connectedId = null;
                MarkVendorDisconnected();
                ErrorOccurred?.Invoke(this,
                    "Vendor scan Connect failed. " + ex.Message);
            }
            finally
            {
                _hband.VendorScanDeviceFound -= OnVendorDeviceFound;
                try
                {
                    await _hband.StopVendorScanAsync(CancellationToken.None).ConfigureAwait(false);
                }
                catch
                {
                    // Best-effort stop after cancel or failure.
                }
            }
        }
        finally
        {
            _connectGate.Release();
        }
    }

    /// <summary>
    /// Drop a Plugin.BLE GATT link so the radio is free for a pure Veepoo scan session.
    /// Does not call vendor disconnect or hybrid settle delays.
    /// </summary>
    private async Task ReleaseActivePluginBleWithoutVendorHandoffAsync(CancellationToken ct)
    {
        if (_usingHbandSession)
        {
            // Vendor already owns the session; do not tear it down for a rescan probe.
            _connectedId = null;
            return;
        }

        if (_connectedId is Guid id && _devices.TryGetValue(id, out var device))
        {
            await ReleasePluginBleLinkAsync(device, ct).ConfigureAwait(false);
            _connectedId = null;
            return;
        }

        var connectedList = _adapter.ConnectedDevices;
        if (connectedList is { Count: > 0 })
        {
            await ReleasePluginBleLinkAsync(connectedList[0], ct).ConfigureAwait(false);
            _connectedId = null;
        }
    }

    public async Task ConnectAsync(Guid deviceId, CancellationToken cancellationToken = default)
    {
        if (!IsBleSupported)
            return;

        await _connectGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var perm = await RequestBluetoothPermissionsAsync(cancellationToken).ConfigureAwait(false);
            if (perm != PermissionStatus.Granted)
            {
                ErrorOccurred?.Invoke(
                    this,
                    "Nearby devices permission is required. Allow it in phone Settings for RaphCare, then try again.");
                return;
            }

            await StopScanAsync().ConfigureAwait(false);

            // A saved row is deliberately not backed by Plugin.BLE. Its purpose is to make
            // the proven Veepoo connect-by-MAC path available when Android returns no scan
            // advertisements for the claimed band.
            if (deviceId == VendorSessionPlaceholderId
                && !string.IsNullOrWhiteSpace(_claimedWatchMac))
            {
                if (!DevicesBleSessionPolicy.PreferExclusiveVendorSession || !_hband.IsAvailable)
                {
                    if (!_hband.IsAvailable)
                    {
                        // Same recovery as auto-reconnect: Plugin.BLE by MAC when AARs did not load.
                        await TryConnectClaimedWatchViaPluginBleAsync(_claimedWatchMac, cancellationToken)
                            .ConfigureAwait(false);
                        if (_connectedId is not null)
                            return;

                        ErrorOccurred?.Invoke(this,
                            "Watch SDK is missing from this install. Reinstall the latest RaphCare APK, or Scan for the watch and Connect from the list.");
                        return;
                    }

                    ErrorOccurred?.Invoke(this,
                        "Watch SDK path is off. Scan for the watch again, then Connect.");
                    return;
                }

                // After a logical Disconnect the vendor session is still up. Reuse it so we
                // never call connectDevice again in this process (that sequence kills the app).
                if (DevicesBleSessionPolicy.ShouldReuseExistingVendorSession(
                        _hband.IsSessionReady,
                        _hband.ConnectedMacAddress,
                        _claimedWatchMac))
                {
                    _connectedId = VendorSessionPlaceholderId;
                    _usingHbandSession = true;
                    _lastSessionMac = _claimedWatchMac;
                    ClearVendorDisconnectSettle();
                    return;
                }

                try
                {
                    // Disconnect → Connect on the Claimed wearable row was killing the process
                    // when Veepoo reconnectDevice ran before Android finished disconnectWatch.
                    await WaitForVendorReconnectSettleAsync(cancellationToken).ConfigureAwait(false);
                    await ReleasePluginBleForMacAsync(_claimedWatchMac, cancellationToken)
                        .ConfigureAwait(false);
                    await SettleAfterScanStopBeforeVendorConnectAsync(cancellationToken)
                        .ConfigureAwait(false);
                    await _hband.ConnectAndHandshakeAsync(
                            _claimedWatchMac,
                            ResolveClaimedDeviceName(),
                            HBandSdkInfo.DefaultDevicePasswordPlaceholder,
                            cancellationToken)
                        .ConfigureAwait(false);
                    _connectedId = VendorSessionPlaceholderId;
                    _usingHbandSession = true;
                    _lastSessionMac = _claimedWatchMac;
                    ClearVendorDisconnectSettle();
                    return;
                }
                catch (Exception hbandEx)
                {
                    _usingHbandSession = false;
                    _connectedId = null;
                    MarkVendorDisconnected();
                    ErrorOccurred?.Invoke(this,
                        "Watch SDK connect failed. Keep the watch nearby and unlocked, then Connect again. "
                        + hbandEx.Message);
                    return;
                }
            }

            if (!_devices.TryGetValue(deviceId, out var device))
            {
                ErrorOccurred?.Invoke(this, "Device is no longer in range. Scan again.");
                return;
            }

            await CleanupNotificationsAsync().ConfigureAwait(false);

            var mac = _macById.GetValueOrDefault(deviceId) ?? TryReadMacAddress(device);
            if (!string.IsNullOrWhiteSpace(mac))
                _macById[deviceId] = mac;

            if (DevicesBleSessionPolicy.ShouldReuseExistingVendorSession(
                    _hband.IsSessionReady,
                    _hband.ConnectedMacAddress,
                    mac))
            {
                _connectedId = device.Id;
                _usingHbandSession = true;
                _lastSessionMac = mac;
                return;
            }

            if (_usingHbandSession || _hband.IsSessionReady)
            {
                try
                {
                    await _hband.DisconnectAsync(cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    // Bridge skips native teardown when no session was started.
                }

                _usingHbandSession = false;
                MarkVendorDisconnected();
            }

            // Exclusive Veepoo session: handshake only (no startDetect on Connect).
            // Dual-stack (Plugin.BLE Connected + Measure handoff) was force-closing the app.
            if (DevicesBleSessionPolicy.PreferExclusiveVendorSession && _hband.IsAvailable)
            {
                if (string.IsNullOrWhiteSpace(mac))
                {
                    ErrorOccurred?.Invoke(
                        this,
                        "Bluetooth address missing. Scan again, then Connect.");
                    return;
                }

                try
                {
                    await WaitForVendorReconnectSettleAsync(cancellationToken).ConfigureAwait(false);
                    await ReleasePluginBleLinkAsync(device, cancellationToken).ConfigureAwait(false);
                    await SettleAfterScanStopBeforeVendorConnectAsync(cancellationToken)
                        .ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(device.Name))
                        _claimedWatchDeviceName = device.Name;
                    await _hband.ConnectAndHandshakeAsync(
                            mac,
                            ResolveClaimedDeviceName(device.Name),
                            HBandSdkInfo.DefaultDevicePasswordPlaceholder,
                            cancellationToken)
                        .ConfigureAwait(false);
                    _connectedId = device.Id;
                    _usingHbandSession = true;
                    _lastSessionMac = mac;
                    ClearVendorDisconnectSettle();
                    return;
                }
                catch (Exception hbandEx)
                {
                    _usingHbandSession = false;
                    _connectedId = null;
                    _lastSessionMac = null;
                    try
                    {
                        await _hband.DisconnectAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch
                    {
                        // ignore
                    }

                    MarkVendorDisconnected();
                    ErrorOccurred?.Invoke(
                        this,
                        "Watch SDK connect failed. Keep the watch nearby and unlocked, then Connect again. "
                        + hbandEx.Message);
                    return;
                }
            }

            await _adapter.ConnectToDeviceAsync(device, ConnectParameters.None, cancellationToken).ConfigureAwait(false);
            _connectedId = device.Id;
            _usingHbandSession = false;
            if (!string.IsNullOrWhiteSpace(mac))
                _lastSessionMac = mac;

            await SubscribeStandardHealthNotifiesAsync(device, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex.Message);
            _connectedId = null;
            _usingHbandSession = false;
            _lastSessionMac = null;
        }
        finally
        {
            _connectGate.Release();
        }
    }

    public async Task ReconnectClaimedWatchAsync(
        string macAddress,
        string? deviceName,
        CancellationToken cancellationToken = default)
    {
        if (!IsBleSupported)
            return;

        var targetMac = BluetoothMacNormalizer.TryNormalize(macAddress);
        if (targetMac is null)
            return;

        _claimedWatchMac = targetMac;
        if (!string.IsNullOrWhiteSpace(deviceName))
            _claimedWatchDeviceName = deviceName.Trim();

        await _connectGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var perm = await CheckBluetoothPermissionsAsync(cancellationToken).ConfigureAwait(false);
            if (perm != PermissionStatus.Granted)
            {
                ErrorOccurred?.Invoke(
                    this,
                    "Nearby devices permission is required. Tap Scan or Connect and allow it when asked.");
                return;
            }

            if (!await EnsureBluetoothAdapterOnAsync(cancellationToken).ConfigureAwait(false))
            {
                ErrorOccurred?.Invoke(this, "Bluetooth is off. Turn it on and try again.");
                return;
            }

            await StopScanAsync().ConfigureAwait(false);
            await CleanupNotificationsAsync().ConfigureAwait(false);

            if (DevicesBleSessionPolicy.ShouldReuseExistingVendorSession(
                    _hband.IsSessionReady,
                    _hband.ConnectedMacAddress,
                    targetMac))
            {
                _usingHbandSession = true;
                _lastSessionMac = targetMac;
                BindConnectedIdFromMac(targetMac);
                ClearVendorDisconnectSettle();
                return;
            }

            if (DevicesBleSessionPolicy.PreferExclusiveVendorSession && _hband.IsAvailable)
            {
                try
                {
                    await WaitForVendorReconnectSettleAsync(cancellationToken).ConfigureAwait(false);
                    await ReleasePluginBleForMacAsync(targetMac, cancellationToken).ConfigureAwait(false);
                    await SettleAfterScanStopBeforeVendorConnectAsync(cancellationToken)
                        .ConfigureAwait(false);
                    await _hband.ConnectAndHandshakeAsync(
                            targetMac,
                            ResolveClaimedDeviceName(deviceName),
                            HBandSdkInfo.DefaultDevicePasswordPlaceholder,
                            cancellationToken)
                        .ConfigureAwait(false);
                    _usingHbandSession = true;
                    _lastSessionMac = targetMac;
                    BindConnectedIdFromMac(targetMac);
                    ClearVendorDisconnectSettle();
                    return;
                }
                catch (Exception hbandEx)
                {
                    _usingHbandSession = false;
                    _connectedId = null;
                    _lastSessionMac = null;
                    try
                    {
                        await _hband.DisconnectAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch
                    {
                        // ignore
                    }

                    MarkVendorDisconnected();
                    ErrorOccurred?.Invoke(
                        this,
                        "Watch SDK connect failed. Keep the watch nearby and unlocked, then Connect again. "
                        + hbandEx.Message);
                    return;
                }
            }

            if (DevicesBleSessionPolicy.ShouldUsePluginBleReconnectWhenVendorUnavailable(_hband.IsAvailable))
            {
                await TryConnectClaimedWatchViaPluginBleAsync(targetMac, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        finally
        {
            _connectGate.Release();
        }
    }

    private async Task TryConnectClaimedWatchViaPluginBleAsync(string targetMac, CancellationToken cancellationToken)
    {
        if (CrossBluetoothLE.Current.State != BluetoothState.On)
            return;

        using var scanCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        scanCts.CancelAfter(DevicesBleSessionPolicy.ClaimedWatchReconnectScanTimeout);
        try
        {
            _devices.Clear();
            _macById.Clear();
            RaiseDiscoveredChanged();
            await ScanIntoCacheAsync(
                    showAllDevices: false,
                    preferredMac: targetMac,
                    timeoutMs: (int)DevicesBleSessionPolicy.ClaimedWatchReconnectScanTimeout.TotalMilliseconds,
                    cancellationToken: scanCts.Token)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Scan window ended. Use whatever was discovered.
        }

        try
        {
            await StopScanAsync().ConfigureAwait(false);
        }
        catch
        {
            // ignore
        }

        Guid? matchId = null;
        foreach (var pair in _macById)
        {
            if (BluetoothMacNormalizer.EqualsNormalized(pair.Value, targetMac))
            {
                matchId = pair.Key;
                break;
            }
        }

        if (matchId is null || !_devices.TryGetValue(matchId.Value, out var device))
            return;

        await _adapter.ConnectToDeviceAsync(device, ConnectParameters.None, cancellationToken)
            .ConfigureAwait(false);
        _connectedId = device.Id;
        _usingHbandSession = false;
        _lastSessionMac = targetMac;
        await SubscribeStandardHealthNotifiesAsync(device, cancellationToken).ConfigureAwait(false);
    }

    private async Task ReleasePluginBleForMacAsync(string targetMac, CancellationToken ct)
    {
        foreach (var pair in _macById)
        {
            if (!BluetoothMacNormalizer.EqualsNormalized(pair.Value, targetMac))
                continue;
            if (!_devices.TryGetValue(pair.Key, out var device))
                continue;
            if (device.State == DeviceState.Disconnected)
                continue;

            await ReleasePluginBleLinkAsync(device, ct).ConfigureAwait(false);
            return;
        }

        if (_connectedId is Guid id
            && _devices.TryGetValue(id, out var connected)
            && connected.State != DeviceState.Disconnected)
        {
            await ReleasePluginBleLinkAsync(connected, ct).ConfigureAwait(false);
        }
    }

    private void BindConnectedIdFromMac(string mac)
    {
        foreach (var pair in _macById)
        {
            if (BluetoothMacNormalizer.EqualsNormalized(pair.Value, mac))
            {
                _connectedId = pair.Key;
                return;
            }
        }

        _connectedId = VendorSessionPlaceholderId;
    }

    private string? TryResolveConnectedMac(Guid? deviceId, IDevice? device)
    {
        if (deviceId is Guid id
            && _macById.TryGetValue(id, out var mapped)
            && !string.IsNullOrWhiteSpace(mapped))
            return mapped;

        if (device is not null)
        {
            var fromDevice = TryReadMacAddress(device);
            if (!string.IsNullOrWhiteSpace(fromDevice))
                return fromDevice;
        }

        var fromBridge = _hband.ConnectedMacAddress;
        if (!string.IsNullOrWhiteSpace(fromBridge))
            return fromBridge;

        return string.IsNullOrWhiteSpace(_lastSessionMac) ? null : _lastSessionMac;
    }

    private async Task EnsureVendorSessionForMeasureAsync(
        IDevice? device,
        string mac,
        string? deviceName,
        CancellationToken ct)
    {
        if (DevicesBleSessionPolicy.ShouldReuseExistingVendorSession(
                _hband.IsSessionReady,
                _hband.ConnectedMacAddress,
                mac))
        {
            _usingHbandSession = true;
            _lastSessionMac = mac;
            if (_connectedId is null)
                BindConnectedIdFromMac(mac);
            return;
        }

        if (device is not null)
            await ReleasePluginBleLinkAsync(device, ct).ConfigureAwait(false);

        using var handshakeCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        handshakeCts.CancelAfter(DevicesBleSessionPolicy.VendorHandshakeTimeout);
        await _hband.ConnectAndHandshakeAsync(
                mac,
                deviceName,
                HBandSdkInfo.DefaultDevicePasswordPlaceholder,
                handshakeCts.Token)
            .ConfigureAwait(false);

        _usingHbandSession = true;
        _lastSessionMac = mac;
        if (device is not null)
            _connectedId = device.Id;
        else
            BindConnectedIdFromMac(mac);
    }

    private async Task SubscribeStandardHealthNotifiesAsync(IDevice device, CancellationToken cancellationToken)
    {
        var services = await device.GetServicesAsync(cancellationToken).ConfigureAwait(false);
        foreach (var service in services)
        {
            var characteristics = await service.GetCharacteristicsAsync().ConfigureAwait(false);
            foreach (var c in characteristics)
            {
                if (!c.CanUpdate)
                    continue;

                var uuidText = c.Uuid.ToString();
                // Only standard HR / pulse-ox notify UUIDs. Subscribing every CanUpdate char
                // on E580/E585 firmware often triggers Android "This feature is not supported" toasts.
                if (!IsStandardHealthNotifyUuid(uuidText))
                    continue;

                EventHandler<CharacteristicUpdatedEventArgs> handler = (_, e) =>
                    OnCharacteristicNotified(e.Characteristic);
                c.ValueUpdated += handler;
                _notifyHandlers.Add((Ch: c, H: handler));
                try
                {
                    await c.StartUpdatesAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Notify failed ({uuidText}): {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// After Measure (success or fail), drop the vendor link and re-open Plugin.BLE GATT so
    /// Devices still shows Connected and the next Measure can hand off cleanly.
    /// </summary>
    private async Task TryRestorePluginBleAfterMeasureAsync(IDevice device, Guid deviceId, CancellationToken ct)
    {
        try
        {
            if (_usingHbandSession)
            {
                try
                {
                    await _hband.DisconnectAsync(ct).ConfigureAwait(false);
                }
                catch
                {
                    // ignore
                }

                _usingHbandSession = false;
            }

            await Task.Delay(DevicesBleSessionPolicy.PostGattDisconnectSettle, ct).ConfigureAwait(false);
            await CleanupNotificationsAsync().ConfigureAwait(false);

            Interlocked.Increment(ref _suppressDisconnectClear);
            try
            {
                await _adapter.ConnectToDeviceAsync(device, ConnectParameters.None, ct).ConfigureAwait(false);
                _connectedId = deviceId;
                await SubscribeStandardHealthNotifiesAsync(device, ct).ConfigureAwait(false);
            }
            finally
            {
                Interlocked.Decrement(ref _suppressDisconnectClear);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Restore Plugin.BLE after Measure: {ex.Message}");
            // Keep _connectedId if we still believe the patient has a claimed watch session intent;
            // Measure can retry vendor connect without requiring a full Devices Connect.
            _connectedId ??= deviceId;
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        await _connectGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await CleanupNotificationsAsync().ConfigureAwait(false);
            var retainedVendorSession = false;
            if (_usingHbandSession || _hband.IsSessionReady)
            {
                // E580/E585 Veepoo can terminate the Android process when disconnectWatch is
                // followed by connectDevice in this process. Preserve its exclusive session;
                // the next Connect safely reuses it while the UI is logically disconnected.
                retainedVendorSession = DevicesBleSessionPolicy.KeepVendorSessionAliveAfterUserDisconnect;
                if (!retainedVendorSession)
                {
                    await _hband.DisconnectAsync(cancellationToken).ConfigureAwait(false);
                    MarkVendorDisconnected();
                }

                _usingHbandSession = false;
            }

            if (_connectedId is { } id && _devices.TryGetValue(id, out var device))
            {
                try
                {
                    await _adapter.DisconnectDeviceAsync(device).ConfigureAwait(false);
                }
                catch
                {
                    // may already be disconnected when HBand owned the link
                }
            }

            _connectedId = null;
            if (!retainedVendorSession)
                _lastSessionMac = null;
            _lastVitals = null;
        }
        finally
        {
            _connectGate.Release();
        }
    }

    public async Task<WearableVitalsSnapshot?> MeasureLiveVitalsAsync(CancellationToken cancellationToken = default)
    {
        IDevice? device = null;
        if (_connectedId is Guid connectedId)
            _devices.TryGetValue(connectedId, out device);

        var deviceId = _connectedId ?? Guid.Empty;
        if (!_usingHbandSession && device is null)
        {
            ErrorOccurred?.Invoke(this, "Connect your claimed watch first, then tap Measure.");
            return null;
        }

        using var overallCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        overallCts.CancelAfter(DevicesBleSessionPolicy.LiveMeasureOverallTimeout);
        var ct = overallCts.Token;

        await _connectGate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await StopScanAsync().ConfigureAwait(false);

            if (!DevicesBleSessionPolicy.EnableVendorLiveMeasure)
            {
                if (!_usingHbandSession && device is { State: DeviceState.Connected })
                {
                    var existing = await TryWaitForExistingGattSampleAsync(ct).ConfigureAwait(false);
                    if (existing is not null
                        && DevicesVitalsDisplayPolicy.HasPatientFacingReading(
                            existing.HeartRateBpm,
                            existing.SpO2Percent))
                        return existing;
                }

                ErrorOccurred?.Invoke(
                    this,
                    DevicesBleSessionPolicy.VendorLiveMeasureDisabledPatientMessage);
                return null;
            }

            if (!_hband.IsAvailable)
            {
                ErrorOccurred?.Invoke(this, "Live measure needs the watch SDK on this phone build.");
                return null;
            }

            var mac = TryResolveConnectedMac(_connectedId, device);
            if (DevicesBleSessionPolicy.ShouldAdoptBridgeVendorSession(
                    _usingHbandSession,
                    _hband.IsSessionReady))
            {
                _usingHbandSession = true;
                if (_connectedId is null && !string.IsNullOrWhiteSpace(mac))
                    BindConnectedIdFromMac(mac);
            }

            // Auto-reconnect / Devices can show Connected on Plugin.BLE while Measure needs Veepoo.
            // Handshake here only when GATT is not holding the radio (no dual-stack steal).
            if (!_usingHbandSession)
            {
                var pluginBleGattConnected = device is { State: DeviceState.Connected };
                if (DevicesBleSessionPolicy.ShouldEstablishVendorSessionForMeasure(
                        DevicesBleSessionPolicy.EnableVendorLiveMeasure,
                        DevicesBleSessionPolicy.PreferExclusiveVendorSession,
                        _hband.IsAvailable,
                        alreadyUsingVendorSession: false,
                        hasBluetoothMac: !string.IsNullOrWhiteSpace(mac),
                        pluginBleGattConnected: pluginBleGattConnected))
                {
                    try
                    {
                        await EnsureVendorSessionForMeasureAsync(
                                device,
                                mac!,
                                device?.Name,
                                ct)
                            .ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                    {
                        ErrorOccurred?.Invoke(
                            this,
                            "Watch connect for Measure timed out. Keep the watch nearby and unlocked, then try again.");
                        return null;
                    }
                    catch (Exception establishEx)
                    {
                        ErrorOccurred?.Invoke(
                            this,
                            "Watch connect for Measure failed. Keep the watch nearby and unlocked, then try again. "
                            + establishEx.Message);
                        return null;
                    }
                }
                else
                {
                    ErrorOccurred?.Invoke(
                        this,
                        pluginBleGattConnected
                            ? "Devices shows Connected on Bluetooth only. Open Devices, tap Disconnect, then Connect again, then Measure."
                            : "Connect your claimed watch on Devices first, then Measure.");
                    return null;
                }
            }

            var tcs = new TaskCompletionSource<WearableVitalsSnapshot>(TaskCreationOptions.RunContinuationsAsynchronously);
            void OnMeasureSample(object? sender, WearableVitalsSnapshot snap)
            {
                if (!snap.HeartRateBpm.HasValue && !snap.SpO2Percent.HasValue)
                    return;
                MergeLastVitals(snap);
                tcs.TrySetResult(snap);
            }

            VitalsUpdated += OnMeasureSample;
            string? blockingHeartMessage = null;
            void OnMeasureError(object? sender, string? message)
            {
                if (string.IsNullOrWhiteSpace(message))
                    return;
                blockingHeartMessage = message;
                tcs.TrySetCanceled(CancellationToken.None);
            }

            _hband.ErrorOccurred += OnMeasureError;
            try
            {
                // Never call stopDetect* before the first startDetect* on this session.
                // Cold stopDetect was force-closing the app ("RaphCare keeps stopping").
                await _hband.StartLiveHeartRateAsync(ct).ConfigureAwait(false);

                using var sampleCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                sampleCts.CancelAfter(DevicesBleSessionPolicy.LiveMeasureTimeout);
                try
                {
                    using var reg = sampleCts.Token.Register(() => tcs.TrySetCanceled(sampleCts.Token));
                    var sample = await tcs.Task.ConfigureAwait(false);

                    try
                    {
                        await _hband.StopLiveDetectionsAsync(ct).ConfigureAwait(false);
                    }
                    catch (Exception stopEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Stop detect: {stopEx.Message}");
                    }

                    if (DevicesBleSessionPolicy.EnableVendorSpo2DuringMeasure)
                    {
                        try
                        {
                            await Task.Delay(TimeSpan.FromSeconds(1), ct).ConfigureAwait(false);
                            await _hband.StartLiveSpo2Async(ct).ConfigureAwait(false);
                            await Task.Delay(TimeSpan.FromSeconds(8), ct).ConfigureAwait(false);
                            await _hband.StopLiveDetectionsAsync(ct).ConfigureAwait(false);
                        }
                        catch (Exception spo2Ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"SpO₂ start: {spo2Ex.Message}");
                        }
                    }

                    return _lastVitals ?? sample;
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    // Wear/busy/battery already raised via ErrorOccurred; do not overwrite with timeout copy.
                    if (string.IsNullOrWhiteSpace(blockingHeartMessage))
                    {
                        ErrorOccurred?.Invoke(
                            this,
                            "No heart rate yet. On the watch open Heart Rate, tap to test, keep it on your wrist, then Measure again.");
                    }

                    return _lastVitals;
                }
            }
            finally
            {
                _hband.ErrorOccurred -= OnMeasureError;
                VitalsUpdated -= OnMeasureSample;
                try
                {
                    await _hband.StopLiveDetectionsAsync(CancellationToken.None).ConfigureAwait(false);
                }
                catch
                {
                    // ignore
                }
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            ErrorOccurred?.Invoke(
                this,
                "Measure timed out. Keep the watch nearby, open Heart Rate on the watch, then try again.");
            return null;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex.Message);
            return null;
        }
        finally
        {
            if (DevicesBleSessionPolicy.RestorePluginBleAfterMeasure
                && device is not null
                && (_usingHbandSession || device.State == DeviceState.Disconnected))
            {
                try
                {
                    using var restoreCts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                    await TryRestorePluginBleAfterMeasureAsync(device, deviceId, restoreCts.Token)
                        .ConfigureAwait(false);
                }
                catch (Exception restoreEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Measure restore: {restoreEx.Message}");
                }
            }

            _connectGate.Release();
        }
    }

    private async Task ReleasePluginBleLinkAsync(IDevice device, CancellationToken ct)
    {
        await CleanupNotificationsAsync().ConfigureAwait(false);
        var wasConnected = device.State != DeviceState.Disconnected;
        Interlocked.Increment(ref _suppressDisconnectClear);
        try
        {
            if (wasConnected)
            {
                try
                {
                    await _adapter.DisconnectDeviceAsync(device).ConfigureAwait(false);
                }
                catch
                {
                    // already disconnected
                }
            }

            if (DevicesBleSessionPolicy.ShouldWaitAfterReleasingPluginBle(wasConnected))
                await Task.Delay(DevicesBleSessionPolicy.PostGattDisconnectSettle, ct).ConfigureAwait(false);
        }
        finally
        {
            Interlocked.Decrement(ref _suppressDisconnectClear);
        }
    }

    private void MarkVendorDisconnected() =>
        _lastVendorDisconnectUtc = DateTimeOffset.UtcNow;

    private void ClearVendorDisconnectSettle() =>
        _lastVendorDisconnectUtc = null;

    private async Task WaitForVendorReconnectSettleAsync(CancellationToken ct)
    {
        var remaining = DevicesBleSessionPolicy.RemainingVendorReconnectSettle(
            _lastVendorDisconnectUtc,
            DateTimeOffset.UtcNow);
        if (remaining > TimeSpan.Zero)
            await Task.Delay(remaining, ct).ConfigureAwait(false);
    }

    private static Task SettleAfterScanStopBeforeVendorConnectAsync(CancellationToken ct)
    {
        var settle = DevicesBleSessionPolicy.PostScanStopSettleBeforeVendorConnect;
        return settle > TimeSpan.Zero
            ? Task.Delay(settle, ct)
            : Task.CompletedTask;
    }

    /// <summary>
    /// Veepoo connectDevice with a null/empty name has crashed on reconnect after Disconnect.
    /// Prefer the last known model/name, else a safe E580 placeholder.
    /// </summary>
    private string ResolveClaimedDeviceName(string? preferred = null)
    {
        if (!string.IsNullOrWhiteSpace(preferred))
            return preferred.Trim();
        if (!string.IsNullOrWhiteSpace(_claimedWatchDeviceName))
            return _claimedWatchDeviceName;
        return "ET580";
    }

    private async Task<WearableVitalsSnapshot?> TryWaitForExistingGattSampleAsync(CancellationToken ct)
    {
        if (_lastVitals is not null
            && DevicesVitalsDisplayPolicy.HasPatientFacingReading(_lastVitals.HeartRateBpm, _lastVitals.SpO2Percent)
            && (DateTimeOffset.UtcNow - _lastVitals.At) < TimeSpan.FromMinutes(2))
        {
            return _lastVitals;
        }

        var tcs = new TaskCompletionSource<WearableVitalsSnapshot>(TaskCreationOptions.RunContinuationsAsynchronously);
        void OnSample(object? sender, WearableVitalsSnapshot snap)
        {
            if (!DevicesVitalsDisplayPolicy.HasPatientFacingReading(snap.HeartRateBpm, snap.SpO2Percent))
                return;
            tcs.TrySetResult(snap);
        }

        VitalsUpdated += OnSample;
        try
        {
            using var waitCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            waitCts.CancelAfter(
                DevicesBleSessionPolicy.EnableVendorLiveMeasure
                    ? TimeSpan.FromSeconds(8)
                    : TimeSpan.FromSeconds(3));
            using var reg = waitCts.Token.Register(() => tcs.TrySetCanceled(waitCts.Token));
            try
            {
                return await tcs.Task.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }
        finally
        {
            VitalsUpdated -= OnSample;
        }
    }

    private void MergeLastVitals(WearableVitalsSnapshot incoming)
    {
        var prev = _lastVitals;
        _lastVitals = new WearableVitalsSnapshot
        {
            At = incoming.At,
            HeartRateBpm = incoming.HeartRateBpm ?? prev?.HeartRateBpm,
            SpO2Percent = incoming.SpO2Percent ?? prev?.SpO2Percent,
            SpO2PulseBpm = incoming.SpO2PulseBpm ?? prev?.SpO2PulseBpm,
            CharacteristicUuid = incoming.CharacteristicUuid,
            RawHex = incoming.RawHex,
        };
    }

    private void OnDeviceDiscovered(object? sender, BleDeviceEventArgs e)
    {
        var mac = TryReadMacAddress(e.Device);
        if (!WearableBleScanRules.IncludeInFilteredScan(
                e.Device.Name,
                mac,
                _scanPreferredMac,
                _scanShowAll))
            return;

        _devices[e.Device.Id] = e.Device;
        if (!string.IsNullOrWhiteSpace(mac))
        {
            _macById[e.Device.Id] = mac!;
            if (_usingHbandSession
                && BluetoothMacNormalizer.EqualsNormalized(_hband.ConnectedMacAddress, mac))
            {
                _connectedId = e.Device.Id;
            }
        }

        MainThread.BeginInvokeOnMainThread(() => DiscoveredDevicesChanged?.Invoke(this, EventArgs.Empty));
    }

    private void OnHbandVitalsUpdated(object? sender, WearableVitalsSnapshot e)
    {
        MergeLastVitals(e);
        MainThread.BeginInvokeOnMainThread(() => VitalsUpdated?.Invoke(this, e));
    }

    private void OnHbandError(object? sender, string? message) =>
        MainThread.BeginInvokeOnMainThread(() => ErrorOccurred?.Invoke(this, message));

    private void OnHbandConnectStepChanged(object? sender, string step) =>
        MainThread.BeginInvokeOnMainThread(() => VendorConnectStepChanged?.Invoke(this, step));

    private static string? TryReadMacAddress(IDevice device)
    {
#if ANDROID
        try
        {
            if (device.NativeDevice is global::Android.Bluetooth.BluetoothDevice bd
                && !string.IsNullOrWhiteSpace(bd.Address))
                return bd.Address;
        }
        catch
        {
            // ignore
        }
#endif
        return null;
    }

    private void RaiseDiscoveredChanged() =>
        MainThread.BeginInvokeOnMainThread(() => DiscoveredDevicesChanged?.Invoke(this, EventArgs.Empty));

    private void OnCharacteristicNotified(ICharacteristic characteristic)
    {
        var value = characteristic.Value;
        if (value is null || value.Length == 0)
            return;

        var uuidText = characteristic.Uuid.ToString();
        var hex = BleGattHeartRateParser.ToHex(value);
        int? hr = null;
        if (IsHeartRateMeasurement(uuidText) && BleGattHeartRateParser.TryParseHeartRateMeasurement(value.AsSpan(), out var bpm))
            hr = bpm;

        decimal? spo2 = null;
        int? spo2Pulse = null;
        var span = value.AsSpan();
        if (IsPlxContinuous(uuidText) && BleGattPulseOximeterParser.TryParsePlxContinuousMeasurement(span, out var spC, out var pulseC))
        {
            spo2 = spC;
            spo2Pulse = pulseC;
        }
        else if (IsPlxSpotCheck(uuidText) && BleGattPulseOximeterParser.TryParsePlxSpotCheckMeasurement(span, out var spS, out var pulseS))
        {
            spo2 = spS;
            spo2Pulse = pulseS;
        }

        var snap = new WearableVitalsSnapshot
        {
            At = DateTimeOffset.UtcNow,
            HeartRateBpm = hr,
            SpO2Percent = spo2,
            SpO2PulseBpm = spo2Pulse,
            CharacteristicUuid = uuidText,
            RawHex = hex,
        };

        // Skip empty vendor noise packets that parse to neither HR nor SpO₂.
        if (!hr.HasValue && !spo2.HasValue)
            return;

        MergeLastVitals(snap);
        MainThread.BeginInvokeOnMainThread(() => VitalsUpdated?.Invoke(this, snap));
    }

    private static bool IsStandardHealthNotifyUuid(string uuidText) =>
        IsHeartRateMeasurement(uuidText) || IsPlxContinuous(uuidText) || IsPlxSpotCheck(uuidText);

    private static bool IsHeartRateMeasurement(string uuidText) =>
        uuidText.Contains("2a37", StringComparison.OrdinalIgnoreCase);

    private static bool IsPlxContinuous(string uuidText) =>
        uuidText.Contains("2a60", StringComparison.OrdinalIgnoreCase);

    private static bool IsPlxSpotCheck(string uuidText) =>
        uuidText.Contains("2a5f", StringComparison.OrdinalIgnoreCase);

    private async Task CleanupNotificationsAsync()
    {
        foreach (var (ch, h) in _notifyHandlers)
        {
            try
            {
                ch.ValueUpdated -= h;
                await ch.StopUpdatesAsync().ConfigureAwait(false);
            }
            catch
            {
                // best-effort teardown
            }
        }

        _notifyHandlers.Clear();
    }

    public void Dispose()
    {
        _hband.VitalsUpdated -= OnHbandVitalsUpdated;
        _hband.ErrorOccurred -= OnHbandError;
        _hband.ConnectStepChanged -= OnHbandConnectStepChanged;
        _connectGate.Dispose();
    }
}
