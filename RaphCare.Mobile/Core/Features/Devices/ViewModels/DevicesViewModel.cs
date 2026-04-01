using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Devices;
using RaphCare.Mobile.Core.Features.Devices.Models;
using RaphCare.Mobile.Core.Features.Devices.Services;
using RaphCare.Mobile.Core.Shared.Navigation;
using RaphCare.Mobile.Core.Shared.ViewModels;
using RaphCare.Mobile.Kernel.Core.Shared.Devices;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Features.Devices.ViewModels;

/// <summary>E580 / E585-class BLE wearables: scan, connect, and display HR or raw notify payloads.</summary>
public sealed class DevicesViewModel : BaseViewModel
{
    private readonly IWearableBleCoordinator _ble;
    private readonly IPatientDevicesService _patientDevices;
    private bool _showAllDevices;
    private bool _isScanningUi;
    private string? _errorMessage;
    private string? _statusHint;
    private WearableVitalsSnapshot? _lastVitals;
    private string _serialNumber = "";
    private string _modelSku = PatientProvisionedDeviceSkus.E585;
    private Guid? _registeredDeviceId;
    private string? _syncResultText;

    public DevicesViewModel(IWearableBleCoordinator ble, IPatientDevicesService patientDevices)
    {
        _ble = ble ?? throw new ArgumentNullException(nameof(ble));
        _patientDevices = patientDevices ?? throw new ArgumentNullException(nameof(patientDevices));
        Title = AppResources.T("DevicesPageTitle");

        ScanCommand = new Command(async () => await ScanAsync().ConfigureAwait(false), () => !IsBusy && _ble.IsBleSupported);
        StopScanCommand = new Command(async () => await StopScanAsync().ConfigureAwait(false), () => _ble.IsScanning);
        ConnectCommand = new Command<Guid>(async id => await ConnectAsync(id).ConfigureAwait(false), _ => !IsBusy);
        DisconnectCommand = new Command(async () => await DisconnectAsync().ConfigureAwait(false), () => _ble.ConnectedDeviceId.HasValue && !IsBusy);
        RegisterCommand = new Command(async () => await RegisterAsync().ConfigureAwait(false), () => !IsBusy);
        SyncLastReadingCommand = new Command(async () => await SyncLastReadingAsync().ConfigureAwait(false), () => !IsBusy);
        BackCommand = new Command(async () => await SafeShellNavigator.GoToAsync(".."));
        ToggleShowAllCommand = new Command(() =>
        {
            ShowAllDevices = !ShowAllDevices;
            OnPropertyChanged(nameof(ShowAllToggleText));
        });

        _ble.DiscoveredDevicesChanged += OnDiscoveredChanged;
        _ble.VitalsUpdated += OnVitalsUpdated;
        _ble.ErrorOccurred += OnBleError;
    }

    public string ScanButtonText => AppResources.T("DevicesScan");
    public string StopScanButtonText => AppResources.T("DevicesStopScan");
    public string ConnectHint => AppResources.T("DevicesConnectHint");
    public string DisconnectButtonText => AppResources.T("DevicesDisconnect");
    public string ShowAllLabel => AppResources.T("DevicesShowAllBle");
    public string E580E585FilterLabel => AppResources.T("DevicesE580E585Filter");
    public string LastReadingLabel => AppResources.T("DevicesLastReading");
    public string BleUnsupportedMessage => AppResources.T("DevicesBleUnsupported");
    public string DevicesConnectLabel => AppResources.T("DevicesConnectButton");

    public string ShowAllToggleText => ShowAllDevices ? ShowAllLabel : E580E585FilterLabel;

    public bool ShowAllDevices
    {
        get => _showAllDevices;
        set
        {
            if (_showAllDevices == value)
                return;
            _showAllDevices = value;
            OnPropertyChanged(nameof(ShowAllDevices));
            OnPropertyChanged(nameof(ShowAllToggleText));
        }
    }

    public bool BleSupported => _ble.IsBleSupported;

    /// <summary>True while a scan session is running (UI; mirrors coordinator when possible).</summary>
    public bool IsScanningUi
    {
        get => _isScanningUi;
        private set
        {
            if (_isScanningUi == value)
                return;
            _isScanningUi = value;
            OnPropertyChanged(nameof(IsScanningUi));
            if (StopScanCommand is Command c)
                c.ChangeCanExecute();
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public string? StatusHint
    {
        get => _statusHint;
        private set => SetProperty(ref _statusHint, value);
    }

    public WearableVitalsSnapshot? LastVitals
    {
        get => _lastVitals;
        private set
        {
            if (_lastVitals == value)
                return;
            _lastVitals = value;
            OnPropertyChanged(nameof(LastVitals));
            OnPropertyChanged(nameof(VitalsHeartLine));
            OnPropertyChanged(nameof(VitalsRawLine));
        }
    }

    public string? VitalsHeartLine =>
        LastVitals?.HeartRateBpm is int b
            ? string.Format(AppResources.T("DevicesHeartRateFormat"), b)
            : null;

    public string? VitalsRawLine =>
        string.IsNullOrEmpty(LastVitals?.RawHex) ? null : string.Format(AppResources.T("DevicesRawHexFormat"), LastVitals!.RawHex);

    public Guid? ConnectedDeviceId => _ble.ConnectedDeviceId;

    public ObservableCollection<WearableDeviceRowViewModel> Items { get; } = new();

    public ObservableCollection<string> ModelSkuOptions { get; } =
        new() { PatientProvisionedDeviceSkus.E585, PatientProvisionedDeviceSkus.E580 };

    public string SerialNumber
    {
        get => _serialNumber;
        set => SetProperty(ref _serialNumber, value);
    }

    public string ModelSku
    {
        get => _modelSku;
        set => SetProperty(ref _modelSku, value);
    }

    public Guid? RegisteredDeviceId
    {
        get => _registeredDeviceId;
        private set => SetProperty(ref _registeredDeviceId, value);
    }

    public string? SyncResultText
    {
        get => _syncResultText;
        private set => SetProperty(ref _syncResultText, value);
    }

    public ICommand ScanCommand { get; }
    public ICommand StopScanCommand { get; }
    public ICommand ConnectCommand { get; }
    public ICommand DisconnectCommand { get; }
    public ICommand RegisterCommand { get; }
    public ICommand SyncLastReadingCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand ToggleShowAllCommand { get; }

    public void DetachBleHandlers()
    {
        _ble.DiscoveredDevicesChanged -= OnDiscoveredChanged;
        _ble.VitalsUpdated -= OnVitalsUpdated;
        _ble.ErrorOccurred -= OnBleError;
    }

    public async Task OnDisappearingAsync()
    {
        try
        {
            await _ble.StopScanAsync().ConfigureAwait(false);
            await _ble.DisconnectAsync().ConfigureAwait(false);
        }
        catch
        {
            // ignore
        }
    }

    private void OnDiscoveredChanged(object? sender, EventArgs e) =>
        MainThread.BeginInvokeOnMainThread(RefreshItems);

    private void OnVitalsUpdated(object? sender, WearableVitalsSnapshot e) =>
        MainThread.BeginInvokeOnMainThread(() => LastVitals = e);

    private void OnBleError(object? sender, string? message) =>
        MainThread.BeginInvokeOnMainThread(() => ErrorMessage = message);

    private void RefreshItems()
    {
        Items.Clear();
        foreach (var d in _ble.DiscoveredDevices)
        {
            Items.Add(new WearableDeviceRowViewModel
            {
                Id = d.Id,
                Title = string.IsNullOrWhiteSpace(d.Name)
                    ? AppResources.T("DevicesUnnamedPeripheral")
                    : d.Name!,
                RssiText = d.Rssi?.ToString() ?? "—",
            });
        }
    }

    private async Task ScanAsync()
    {
        if (!_ble.IsBleSupported)
        {
            ErrorMessage = BleUnsupportedMessage;
            return;
        }

        ErrorMessage = null;
        StatusHint = AppResources.T("DevicesPermissionChecking");
        IsBusy = true;
        try
        {
            var perm = await _ble.RequestBluetoothPermissionsAsync().ConfigureAwait(false);
            if (perm != PermissionStatus.Granted)
            {
                ErrorMessage = AppResources.T("DevicesPermissionDenied");
                return;
            }

            if (!await _ble.EnsureBluetoothAdapterOnAsync().ConfigureAwait(false))
            {
                ErrorMessage = AppResources.T("DevicesBluetoothOff");
                return;
            }

            IsScanningUi = true;
            StatusHint = AppResources.T("DevicesScanning");
            await _ble.StartScanAsync(ShowAllDevices).ConfigureAwait(false);
            StatusHint = AppResources.T("DevicesScanComplete");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsScanningUi = false;
            IsBusy = false;
            RefreshItems();
            if (ScanCommand is Command sc)
                sc.ChangeCanExecute();
            if (StopScanCommand is Command st)
                st.ChangeCanExecute();
        }
    }

    private async Task StopScanAsync()
    {
        await _ble.StopScanAsync().ConfigureAwait(false);
        IsScanningUi = false;
        StatusHint = AppResources.T("DevicesScanStopped");
        if (StopScanCommand is Command st)
            st.ChangeCanExecute();
    }

    private async Task ConnectAsync(Guid deviceId)
    {
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            await _ble.ConnectAsync(deviceId).ConfigureAwait(false);
            OnPropertyChanged(nameof(ConnectedDeviceId));
            StatusHint = AppResources.T("DevicesConnected");
            SyncResultText = null;

            // Best-effort default SKU from advertised name
            var connected = _ble.DiscoveredDevices.FirstOrDefault(d => d.Id == deviceId);
            var name = connected?.Name ?? "";
            if (name.Contains("E580", StringComparison.OrdinalIgnoreCase))
                ModelSku = PatientProvisionedDeviceSkus.E580;
            else if (name.Contains("E585", StringComparison.OrdinalIgnoreCase))
                ModelSku = PatientProvisionedDeviceSkus.E585;

            if (DisconnectCommand is Command d)
                d.ChangeCanExecute();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DisconnectAsync()
    {
        IsBusy = true;
        try
        {
            await _ble.DisconnectAsync().ConfigureAwait(false);
            OnPropertyChanged(nameof(ConnectedDeviceId));
            LastVitals = null;
            SyncResultText = null;
            StatusHint = AppResources.T("DevicesDisconnected");
            if (DisconnectCommand is Command d)
                d.ChangeCanExecute();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RegisterAsync()
    {
        ErrorMessage = null;
        SyncResultText = null;
        IsBusy = true;
        try
        {
            var serial = (SerialNumber ?? "").Trim();
            if (string.IsNullOrWhiteSpace(serial))
            {
                ErrorMessage = "Enter the device serial number first.";
                return;
            }

            var sku = (ModelSku ?? "").Trim();
            if (string.IsNullOrWhiteSpace(sku))
            {
                ErrorMessage = "Select a model SKU (E580 / E585).";
                return;
            }

            var resp = await _patientDevices.RegisterMyDeviceAsync(serial, sku).ConfigureAwait(false);
            if (!resp.IsSuccess)
            {
                ErrorMessage = resp.ErrorMessage ?? "Device registration failed.";
                return;
            }

            RegisteredDeviceId = resp.Data?.DeviceId;
            SyncResultText = RegisteredDeviceId is { } id ? $"Registered: {id}" : "Registered.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SyncLastReadingAsync()
    {
        ErrorMessage = null;
        SyncResultText = null;
        IsBusy = true;
        try
        {
            if (RegisteredDeviceId is not { } deviceId)
            {
                ErrorMessage = "Register the device first.";
                return;
            }

            var last = LastVitals;
            if (last?.HeartRateBpm is not int bpm)
            {
                ErrorMessage = "No heart rate reading yet. Connect and wait for a reading.";
                return;
            }

            var hr = new List<HeartRateReadingInput>
            {
                new() { RecordedAt = last.At.UtcDateTime, BeatsPerMinute = bpm }
            };

            var resp = await _patientDevices.SyncReadingsAsync(
                deviceId,
                hr,
                Array.Empty<Spo2ReadingInput>(),
                CancellationToken.None).ConfigureAwait(false);

            if (!resp.IsSuccess)
            {
                ErrorMessage = resp.ErrorMessage ?? "Sync failed.";
                return;
            }

            var dto = resp.Data;
            SyncResultText = dto is null
                ? "Synced."
                : $"Synced HR={dto.HeartRateCount}, SpO₂={dto.SpO2Count}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
