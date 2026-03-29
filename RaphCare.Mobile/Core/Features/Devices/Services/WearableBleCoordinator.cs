using System.Collections.Concurrent;
using Microsoft.Maui.ApplicationModel;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using RaphCare.Mobile.Core.Features.Devices.Models;
using BleDeviceEventArgs = Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs;

namespace RaphCare.Mobile.Core.Features.Devices.Services;

/// <summary>
/// BLE scan or connect for E580/E585-class bracelets via Plugin.BLE.
/// Subscribes to all notify characteristics; parses standard Heart Rate (0x2A37) when present, otherwise surfaces raw hex for OEM analysis.
/// </summary>
public sealed class WearableBleCoordinator : IWearableBleCoordinator
{
    private readonly IAdapter _adapter = CrossBluetoothLE.Current.Adapter;
    private readonly ConcurrentDictionary<Guid, IDevice> _devices = new();
    private readonly List<(ICharacteristic Ch, EventHandler<CharacteristicUpdatedEventArgs> H)> _notifyHandlers = new();
    private readonly SemaphoreSlim _connectGate = new(1, 1);

    private Guid? _connectedId;

    public WearableBleCoordinator()
    {
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
            .Select(d => new WearableDeviceDisplayItem { Id = d.Id, Name = d.Name, Rssi = d.Rssi })
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
        RaiseDiscoveredChanged();

        _adapter.ScanTimeout = 30_000;
        _adapter.DeviceDiscovered -= OnDeviceDiscovered;
        _adapter.DeviceDiscovered += OnDeviceDiscovered;

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

            await _adapter.ConnectToDeviceAsync(device, ConnectParameters.None, cancellationToken).ConfigureAwait(false);
            _connectedId = device.Id;

            var services = await device.GetServicesAsync(cancellationToken).ConfigureAwait(false);
            foreach (var service in services)
            {
                var characteristics = await service.GetCharacteristicsAsync().ConfigureAwait(false);
                foreach (var c in characteristics)
                {
                    if (!c.CanUpdate)
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
                        ErrorOccurred?.Invoke(this, $"Notify failed: {ex.Message}");
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
            if (_connectedId is { } id && _devices.TryGetValue(id, out var device))
                await _adapter.DisconnectDeviceAsync(device).ConfigureAwait(false);
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
        MainThread.BeginInvokeOnMainThread(() => DiscoveredDevicesChanged?.Invoke(this, EventArgs.Empty));
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

        var snap = new WearableVitalsSnapshot
        {
            At = DateTimeOffset.UtcNow,
            HeartRateBpm = hr,
            CharacteristicUuid = uuidText,
            RawHex = hex,
        };

        MainThread.BeginInvokeOnMainThread(() => VitalsUpdated?.Invoke(this, snap));
    }

    private static bool IsHeartRateMeasurement(string uuidText) =>
        uuidText.Contains("2a37", StringComparison.OrdinalIgnoreCase);

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
}
