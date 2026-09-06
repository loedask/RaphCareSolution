using System.Collections.Concurrent;
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

    private Guid? _connectedId;
    private bool _usingHbandSession;

    public WearableBleCoordinator(IHBandWearableBridge hband)
    {
        _hband = hband;
        _hband.VitalsUpdated += OnHbandVitalsUpdated;
        _hband.ErrorOccurred += OnHbandError;

        _adapter.DeviceDisconnected += (_, e) =>
        {
            if (_connectedId != e.Device.Id)
                return;
            _connectedId = null;
            _ = Task.Run(async () => await CleanupNotificationsAsync().ConfigureAwait(false));
        };
    }

    public bool IsBleSupported => BlePlatform.IsSupported;

    public bool IsScanning => _adapter.IsScanning;

    public Guid? ConnectedDeviceId => _connectedId;

    public IReadOnlyList<WearableDeviceDisplayItem> DiscoveredDevices =>
        _devices.Values
            .Select(d => new WearableDeviceDisplayItem
            {
                Id = d.Id,
                Name = d.Name,
                Rssi = d.Rssi,
                MacAddress = _macById.GetValueOrDefault(d.Id)
            })
            .OrderByDescending(x => x.Rssi ?? int.MinValue)
            .ToList();

    public event EventHandler? DiscoveredDevicesChanged;
    public event EventHandler<WearableVitalsSnapshot>? VitalsUpdated;
    public event EventHandler<string?>? ErrorOccurred;

    public async Task<PermissionStatus> RequestBluetoothPermissionsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await Permissions.RequestAsync<BluetoothPermissions>().ConfigureAwait(false);
    }

    public Task<bool> EnsureBluetoothAdapterOnAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!IsBleSupported)
            return Task.FromResult(false);
        return Task.FromResult(CrossBluetoothLE.Current.State == BluetoothState.On);
    }

    public async Task StartScanAsync(bool showAllDevices, CancellationToken cancellationToken = default)
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
        _devices.Clear();
        _macById.Clear();
        RaiseDiscoveredChanged();

        _adapter.ScanTimeout = 30_000;
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
                // Best-effort stop when the Devices page cancels the scan.
            }
        });

        try
        {
            await _adapter.StartScanningForDevicesAsync(
                    new ScanFilterOptions(),
                    d => showAllDevices || E585E580DeviceFilter.Matches(d.Name),
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
            ErrorOccurred?.Invoke(this, ex.Message);
        }
        finally
        {
            _adapter.DeviceDiscovered -= OnDeviceDiscovered;
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

    public async Task ConnectAsync(Guid deviceId, CancellationToken cancellationToken = default)
    {
        if (!IsBleSupported)
            return;

        await _connectGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await StopScanAsync().ConfigureAwait(false);

            if (!_devices.TryGetValue(deviceId, out var device))
            {
                ErrorOccurred?.Invoke(this, "Device is no longer in range. Scan again.");
                return;
            }

            await CleanupNotificationsAsync().ConfigureAwait(false);
            if (_usingHbandSession)
            {
                await _hband.DisconnectAsync(cancellationToken).ConfigureAwait(false);
                _usingHbandSession = false;
            }

            var mac = _macById.GetValueOrDefault(deviceId) ?? TryReadMacAddress(device);
            if (!string.IsNullOrWhiteSpace(mac))
                _macById[deviceId] = mac;

            if (DevicesBleSessionPolicy.TryVendorSdkOnConnect
                && _hband.IsAvailable
                && !string.IsNullOrWhiteSpace(mac))
            {
                try
                {
                    await _hband.ConnectAndHandshakeAsync(
                            mac,
                            device.Name,
                            HBandSdkInfo.DefaultDevicePasswordPlaceholder,
                            cancellationToken)
                        .ConfigureAwait(false);
                    _connectedId = device.Id;
                    _usingHbandSession = true;

                    // Live vitals via vendor protocol (HR first, then SpO₂).
                    await _hband.StartLiveHeartRateAsync(cancellationToken).ConfigureAwait(false);
                    try
                    {
                        await _hband.StartLiveSpo2Async(cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception spo2Ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"SpO₂ start: {spo2Ex.Message}");
                    }

                    return;
                }
                catch (Exception hbandEx)
                {
                    // Vendor session often fails on sample bands; fall through to GATT quietly.
                    System.Diagnostics.Debug.WriteLine(
                        $"HBand connect failed, trying standard BLE: {hbandEx.Message}");
                    _usingHbandSession = false;
                    try
                    {
                        await _hband.DisconnectAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            await _adapter.ConnectToDeviceAsync(device, ConnectParameters.None, cancellationToken).ConfigureAwait(false);
            _connectedId = device.Id;
            _usingHbandSession = false;

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
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex.Message);
            _connectedId = null;
            _usingHbandSession = false;
        }
        finally
        {
            _connectGate.Release();
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        await _connectGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await CleanupNotificationsAsync().ConfigureAwait(false);
            if (_usingHbandSession)
            {
                await _hband.DisconnectAsync(cancellationToken).ConfigureAwait(false);
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
        }
        finally
        {
            _connectGate.Release();
        }
    }

    private void OnDeviceDiscovered(object? sender, BleDeviceEventArgs e)
    {
        _devices[e.Device.Id] = e.Device;
        var mac = TryReadMacAddress(e.Device);
        if (!string.IsNullOrWhiteSpace(mac))
            _macById[e.Device.Id] = mac;
        MainThread.BeginInvokeOnMainThread(() => DiscoveredDevicesChanged?.Invoke(this, EventArgs.Empty));
    }

    private void OnHbandVitalsUpdated(object? sender, WearableVitalsSnapshot e) =>
        MainThread.BeginInvokeOnMainThread(() => VitalsUpdated?.Invoke(this, e));

    private void OnHbandError(object? sender, string? message) =>
        MainThread.BeginInvokeOnMainThread(() => ErrorOccurred?.Invoke(this, message));

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
        _connectGate.Dispose();
    }
}
