using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Media;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Devices;
using RaphCare.Client.Models.Fleet;
using RaphCare.Mobile.Core.Features.Devices.Models;
using RaphCare.Mobile.Core.Features.Devices.Services;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;
using RaphCare.Mobile.Kernel.Core.Common.Devices;

namespace RaphCare.Mobile.Core.Features.Devices.ViewModels;

/// <summary>E580 / E585-class BLE wearables: scan, connect, and display HR or raw notify payloads.</summary>
public sealed class DevicesViewModel : BaseViewModel, IDisposable
{
    private readonly IWearableBleCoordinator _ble;
    private readonly IPatientDevicesService _patientDevices;
    private readonly IVitalsSyncOutbox _vitalsOutbox;
    private bool _showAllDevices;
    private bool _isScanningUi;
    private string? _errorMessage;
    private string? _statusHint;
    private WearableVitalsSnapshot? _lastVitals;
    private DateTimeOffset? _lastHeartAt;
    private DateTimeOffset? _lastSpo2At;
    private string _serialNumber = "";
    private string _modelSku = PatientProvisionedDeviceSkus.E585;
    private Guid? _registeredDeviceId;
    private string? _claimedBluetoothMac;
    private string? _syncResultText;
    private CancellationTokenSource? _scanCts;

    public DevicesViewModel(IWearableBleCoordinator ble, IPatientDevicesService patientDevices, IVitalsSyncOutbox vitalsOutbox)
    {
        _ble = ble ?? throw new ArgumentNullException(nameof(ble));
        _patientDevices = patientDevices ?? throw new ArgumentNullException(nameof(patientDevices));
        _vitalsOutbox = vitalsOutbox ?? throw new ArgumentNullException(nameof(vitalsOutbox));
        Title = T("DevicesPageTitle");

        ScanButtonText = T("DevicesScan");
        StopScanButtonText = T("DevicesStopScan");
        ConnectHint = T("DevicesConnectHint");
        DisconnectButtonText = T("DevicesDisconnect");
        ShowAllLabel = T("DevicesShowAllBle");
        E580E585FilterLabel = T("DevicesE580E585Filter");
        LastReadingLabel = T("DevicesLastReading");
        StatusSectionTitle = T("DevicesStatusSectionTitle");
        BluetoothSectionTitle = T("DevicesBluetoothSectionTitle");
        NearbySectionTitle = T("DevicesNearbySectionTitle");
        ClaimSectionTitle = T("DevicesClaimSectionTitle");
        ReadingsEmptyHint = T("DevicesReadingsEmptyHint");
        VitalsWaitingHint = T("DevicesVitalsWaitingHint");
        BleUnsupportedMessage = T("DevicesBleUnsupported");
        DevicesConnectLabel = T("DevicesConnectButton");
        DevicesConnectedRowLabel = T("DevicesConnectedRowButton");

        SyncReadingsButtonText = T("DevicesSyncReadings");
        ClaimHint = T("DevicesClaimHint");
        ClaimButtonText = T("DevicesClaimButton");
        ScanPackagingButtonText = T("DevicesScanPackagingButton");
        SerialPlaceholder = T("DevicesSerialPlaceholder");
        SkuPickerTitle = T("DevicesSkuTitle");

        ScanCommand = new Command(async () => await ScanAsync().ConfigureAwait(false), () => CanScanOrConnect);
        StopScanCommand = new Command(
            async () => await StopScanAsync().ConfigureAwait(false),
            () => DevicesBleSessionPolicy.CanStopScan(IsScanningUi, _ble.IsScanning));
        ConnectCommand = new Command<Guid>(
            async id => await ConnectAsync(id).ConfigureAwait(false),
            id => CanScanOrConnect && id != _ble.ConnectedDeviceId);
        DisconnectCommand = new Command(async () => await DisconnectAsync().ConfigureAwait(false), () => _ble.ConnectedDeviceId.HasValue && !IsBusy);
        RegisterCommand = new Command(async () => await RegisterAsync().ConfigureAwait(false), () => !IsBusy);
        ScanPackagingCommand = new Command(async () => await ScanPackagingAsync().ConfigureAwait(false), () => !IsBusy);
        SyncLastReadingCommand = new Command(async () => await SyncLastReadingAsync().ConfigureAwait(false), () => !IsBusy);
        BackCommand = new Command(async () => await SafeShellNavigator.GoToAsync(".."));
        ToggleShowAllCommand = new Command(() =>
        {
            ShowAllDevices = !ShowAllDevices;
            OnPropertyChanged(nameof(ShowAllToggleText));
            OnPropertyChanged(nameof(VitalsRawLine));
            OnPropertyChanged(nameof(ShowReadingsEmpty));
            OnPropertyChanged(nameof(ShowVitalsWaiting));
        });

        _ble.DiscoveredDevicesChanged += OnDiscoveredChanged;
        _ble.VitalsUpdated += OnVitalsUpdated;
        _ble.ErrorOccurred += OnBleError;
    }

    public string ScanButtonText { get; }
    public string StopScanButtonText { get; }
    public string ConnectHint { get; }
    public string DisconnectButtonText { get; }
    public string ShowAllLabel { get; }
    public string E580E585FilterLabel { get; }
    public string LastReadingLabel { get; }
    public string StatusSectionTitle { get; }
    public string BluetoothSectionTitle { get; }
    public string NearbySectionTitle { get; }
    public string ClaimSectionTitle { get; }
    public string ReadingsEmptyHint { get; }
    public string VitalsWaitingHint { get; }
    public string BleUnsupportedMessage { get; }
    public string DevicesConnectLabel { get; }
    public string DevicesConnectedRowLabel { get; }

    public string SyncReadingsButtonText { get; }
    public string ClaimHint { get; }
    public string ClaimButtonText { get; }
    public string ScanPackagingButtonText { get; }
    public string SerialPlaceholder { get; }
    public string SkuPickerTitle { get; }

    public string ClaimedDeviceIdLabel =>
        RegisteredDeviceId is Guid id ? Format(T("DevicesClaimedIdFormat"), id) : string.Empty;

    public bool HasClaimedDevice => RegisteredDeviceId.HasValue;

    public bool CanScanOrConnect => !IsBusy && _ble.IsBleSupported && HasClaimedDevice;

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
            RaiseCanExecuteChanged(StopScanCommand);
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
            OnPropertyChanged(nameof(VitalsSpo2Line));
            OnPropertyChanged(nameof(VitalsRawLine));
            OnPropertyChanged(nameof(ShowReadingsEmpty));
            OnPropertyChanged(nameof(ShowVitalsWaiting));
        }
    }

    public string? VitalsHeartLine =>
        LastVitals?.HeartRateBpm is int b
            ? Format(T("DevicesHeartRateFormat"), b)
            : null;

    public string? VitalsSpo2Line =>
        LastVitals?.SpO2Percent is decimal sp
            ? Format(T("DevicesSpO2Format"), sp)
            : null;

    public string? VitalsRawLine =>
        DevicesVitalsDisplayPolicy.ShowRawHexToPatient(ShowAllDevices)
        && !string.IsNullOrEmpty(LastVitals?.RawHex)
            ? Format(T("DevicesRawHexFormat"), LastVitals!.RawHex)
            : null;

    public bool ShowReadingsEmpty =>
        !IsBleConnected
        && !DevicesVitalsDisplayPolicy.HasPatientFacingReading(LastVitals?.HeartRateBpm, LastVitals?.SpO2Percent)
        && string.IsNullOrEmpty(VitalsRawLine);

    public bool ShowVitalsWaiting =>
        IsBleConnected
        && !DevicesVitalsDisplayPolicy.HasPatientFacingReading(LastVitals?.HeartRateBpm, LastVitals?.SpO2Percent)
        && string.IsNullOrEmpty(VitalsRawLine);

    public bool ShowNearbyEmpty => Items.Count == 0;

    public Guid? ConnectedDeviceId => _ble.ConnectedDeviceId;

    /// <summary>True while the singleton BLE coordinator still has an active peripheral.</summary>
    public bool IsBleConnected => _ble.ConnectedDeviceId.HasValue;

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
        private set
        {
            if (!SetProperty(ref _registeredDeviceId, value))
                return;
            OnPropertyChanged(nameof(ClaimedDeviceIdLabel));
            OnPropertyChanged(nameof(HasClaimedDevice));
            OnPropertyChanged(nameof(CanScanOrConnect));
            RaiseCanExecuteChanged(ScanCommand, ConnectCommand);
        }
    }

    public string? ClaimedBluetoothMac
    {
        get => _claimedBluetoothMac;
        private set => SetProperty(ref _claimedBluetoothMac, value);
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
    public ICommand ScanPackagingCommand { get; }
    public ICommand SyncLastReadingCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand ToggleShowAllCommand { get; }

    public void DetachBleHandlers()
    {
        _ble.DiscoveredDevicesChanged -= OnDiscoveredChanged;
        _ble.VitalsUpdated -= OnVitalsUpdated;
        _ble.ErrorOccurred -= OnBleError;
        DisposeScanCts();
    }

    public void Dispose()
    {
        DetachBleHandlers();
        GC.SuppressFinalize(this);
    }

    private void DisposeScanCts()
    {
        try
        {
            _scanCts?.Cancel();
        }
        catch
        {
            // ignore
        }

        _scanCts?.Dispose();
        _scanCts = null;
    }

    public async Task OnAppearingAsync()
    {
        try
        {
            var flushed = await _vitalsOutbox.TryFlushAsync(_patientDevices, CancellationToken.None).ConfigureAwait(false);
            if (flushed > 0)
                SyncResultText = Format(T("DevicesOutboxFlushedFormat"), flushed);
        }
        catch
        {
            // Outbox flush is best-effort.
        }

        await LoadClaimedDevicesAsync().ConfigureAwait(false);
        ApplyActiveBleConnectionToUi();
    }

    private async Task LoadClaimedDevicesAsync()
    {
        try
        {
            var resp = await _patientDevices.GetMyDevicesAsync(CancellationToken.None).ConfigureAwait(false);
            if (!resp.IsSuccess || resp.Data is null || resp.Data.Count == 0)
            {
                if (!HasClaimedDevice && !_ble.ConnectedDeviceId.HasValue)
                    StatusHint = T("DevicesClaimBeforeConnectHint");
                return;
            }

            var first = resp.Data[0];
            RegisteredDeviceId = first.DeviceId;
            ClaimedBluetoothMac = BluetoothMacNormalizer.TryNormalize(first.BluetoothMacAddress);
            if (string.IsNullOrWhiteSpace(SerialNumber))
                SerialNumber = first.SerialNumber ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(first.Model)
                && ModelSkuOptions.Contains(first.Model))
                ModelSku = first.Model;
            if (!_ble.ConnectedDeviceId.HasValue)
            {
                StatusHint = string.IsNullOrWhiteSpace(ClaimedBluetoothMac)
                    ? T("DevicesClaimReadyFirstPairHint")
                    : Format(T("DevicesClaimReadyMacHint"), ClaimedBluetoothMac!);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    /// <summary>
    /// Stop scanning when leaving the page. Keep the GATT link (singleton coordinator)
    /// so returning still shows Connected.
    /// </summary>
    public async Task OnDisappearingAsync()
    {
        try
        {
            DisposeScanCts();
            await _ble.StopScanAsync().ConfigureAwait(false);
            if (DevicesBleSessionPolicy.DisconnectWhenLeavingDevicesPage)
                await _ble.DisconnectAsync().ConfigureAwait(false);
        }
        catch
        {
            // ignore
        }
    }

    private void ApplyActiveBleConnectionToUi()
    {
        OnPropertyChanged(nameof(ConnectedDeviceId));
        OnPropertyChanged(nameof(IsBleConnected));
        OnPropertyChanged(nameof(ShowVitalsWaiting));
        OnPropertyChanged(nameof(ShowReadingsEmpty));
        if (_ble.ConnectedDeviceId.HasValue)
            StatusHint = T("DevicesConnected");
        RefreshItems();
        RaiseCanExecuteChanged(ScanCommand, StopScanCommand, ConnectCommand, DisconnectCommand);
    }

    private void OnDiscoveredChanged(object? sender, EventArgs e) =>
        MainThread.BeginInvokeOnMainThread(RefreshItems);

    private void OnVitalsUpdated(object? sender, WearableVitalsSnapshot e) =>
        MainThread.BeginInvokeOnMainThread(() => MergeVitals(e));

    private void MergeVitals(WearableVitalsSnapshot incoming)
    {
        if (incoming.HeartRateBpm.HasValue)
            _lastHeartAt = incoming.At;
        if (incoming.SpO2Percent.HasValue)
            _lastSpo2At = incoming.At;

        var prev = LastVitals;
        LastVitals = new WearableVitalsSnapshot
        {
            At = incoming.At,
            HeartRateBpm = incoming.HeartRateBpm ?? prev?.HeartRateBpm,
            SpO2Percent = incoming.SpO2Percent ?? prev?.SpO2Percent,
            SpO2PulseBpm = incoming.SpO2PulseBpm ?? prev?.SpO2PulseBpm,
            CharacteristicUuid = incoming.CharacteristicUuid,
            RawHex = incoming.RawHex,
        };
    }

    private void OnBleError(object? sender, string? message) =>
        MainThread.BeginInvokeOnMainThread(() => ErrorMessage = message);

    private void RefreshItems()
    {
        Items.Clear();
        var connectedId = _ble.ConnectedDeviceId;
        foreach (var d in _ble.DiscoveredDevices)
        {
            var isConnected = connectedId.HasValue && d.Id == connectedId.Value;
            Items.Add(new WearableDeviceRowViewModel
            {
                Id = d.Id,
                Title = string.IsNullOrWhiteSpace(d.Name)
                    ? T("DevicesUnnamedPeripheral")
                    : d.Name!,
                RssiText = d.Rssi?.ToString(CultureInfo.InvariantCulture) ?? "-",
                IsConnected = isConnected,
                ActionLabel = isConnected ? DevicesConnectedRowLabel : DevicesConnectLabel,
            });
        }

        OnPropertyChanged(nameof(ShowNearbyEmpty));
        OnPropertyChanged(nameof(ShowVitalsWaiting));
        OnPropertyChanged(nameof(ShowReadingsEmpty));
    }

    private async Task ScanAsync()
    {
        if (!_ble.IsBleSupported)
        {
            ErrorMessage = BleUnsupportedMessage;
            return;
        }

        if (!HasClaimedDevice)
        {
            ErrorMessage = T("DevicesClaimBeforeConnect");
            return;
        }

        ErrorMessage = null;
        StatusHint = T("DevicesPermissionChecking");
        IsBusy = true;
        try
        {
            var perm = await _ble.RequestBluetoothPermissionsAsync().ConfigureAwait(false);
            if (perm != PermissionStatus.Granted)
            {
                ErrorMessage = T("DevicesPermissionDenied");
                return;
            }

            if (!await _ble.EnsureBluetoothAdapterOnAsync().ConfigureAwait(false))
            {
                ErrorMessage = T("DevicesBluetoothOff");
                return;
            }

            IsScanningUi = true;
            StatusHint = T("DevicesScanning");
            DisposeScanCts();
            _scanCts = new CancellationTokenSource();
            var scanToken = _scanCts.Token;
            try
            {
                await _ble.StartScanAsync(ShowAllDevices, scanToken).ConfigureAwait(false);
                StatusHint = scanToken.IsCancellationRequested
                    ? T("DevicesScanStopped")
                    : T("DevicesScanComplete");
            }
            catch (OperationCanceledException)
            {
                StatusHint = T("DevicesScanStopped");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsScanningUi = false;
            IsBusy = false;
            await RunOnMainThreadAsync(RefreshItems).ConfigureAwait(false);
            RaiseCanExecuteChanged(ScanCommand, StopScanCommand, ConnectCommand, DisconnectCommand);
        }
    }

    private async Task StopScanAsync()
    {
        try
        {
            try
            {
                _scanCts?.Cancel();
            }
            catch
            {
                // ignore
            }

            await _ble.StopScanAsync().ConfigureAwait(false);
            IsScanningUi = false;
            StatusHint = T("DevicesScanStopped");
        }
        finally
        {
            await RunOnMainThreadAsync(RefreshItems).ConfigureAwait(false);
            RaiseCanExecuteChanged(ScanCommand, StopScanCommand, ConnectCommand, DisconnectCommand);
        }
    }

    private async Task ConnectAsync(Guid deviceId)
    {
        ErrorMessage = null;
        if (!HasClaimedDevice || RegisteredDeviceId is not Guid claimedId)
        {
            ErrorMessage = T("DevicesClaimBeforeConnect");
            return;
        }

        var peripheral = _ble.DiscoveredDevices.FirstOrDefault(d => d.Id == deviceId);
        var peripheralMac = BluetoothMacNormalizer.TryNormalize(peripheral?.MacAddress);
        var expectedMac = BluetoothMacNormalizer.TryNormalize(ClaimedBluetoothMac);

        if (!ClaimedWatchConnectGate.AllowsConnect(ClaimedBluetoothMac, peripheral?.MacAddress))
        {
            ErrorMessage = Format(
                T("DevicesWrongWatchMac"),
                expectedMac ?? ClaimedBluetoothMac ?? "");
            return;
        }

        IsBusy = true;
        try
        {
            await _ble.ConnectAsync(deviceId).ConfigureAwait(false);
            ErrorMessage = null;
            OnPropertyChanged(nameof(ConnectedDeviceId));
            OnPropertyChanged(nameof(IsBleConnected));
            OnPropertyChanged(nameof(ShowVitalsWaiting));
            OnPropertyChanged(nameof(ShowReadingsEmpty));
            StatusHint = T("DevicesConnected");
            SyncResultText = null;

            // Best-effort default SKU from advertised name (ET580/ET585 on-device labels included)
            var connected = _ble.DiscoveredDevices.FirstOrDefault(d => d.Id == deviceId);
            var name = connected?.Name ?? "";
            if (name.Contains("ET580", StringComparison.OrdinalIgnoreCase)
                || name.Contains("E580", StringComparison.OrdinalIgnoreCase))
                ModelSku = PatientProvisionedDeviceSkus.E580;
            else if (name.Contains("ET585", StringComparison.OrdinalIgnoreCase)
                     || name.Contains("E585", StringComparison.OrdinalIgnoreCase))
                ModelSku = PatientProvisionedDeviceSkus.E585;

            if (expectedMac is null)
            {
                var learned = peripheralMac
                    ?? BluetoothMacNormalizer.TryNormalize(_ble.ConnectedDeviceId.HasValue
                        ? _ble.DiscoveredDevices.FirstOrDefault(d => d.Id == _ble.ConnectedDeviceId)?.MacAddress
                        : null);

                if (learned is null)
                {
                    ErrorMessage = T("DevicesMacMissingAfterConnect");
                }
                else
                {
                    var bind = await _patientDevices.BindBluetoothMacAsync(claimedId, learned, CancellationToken.None)
                        .ConfigureAwait(false);
                    if (!bind.IsSuccess)
                    {
                        ErrorMessage = bind.ErrorMessage ?? T("DevicesMacBindFailed");
                        try
                        {
                            await _ble.DisconnectAsync().ConfigureAwait(false);
                        }
                        catch
                        {
                            // ignore
                        }

                        OnPropertyChanged(nameof(ConnectedDeviceId));
                        OnPropertyChanged(nameof(IsBleConnected));
                    }
                    else
                    {
                        ClaimedBluetoothMac = bind.Data?.BluetoothMacAddress ?? learned;
                        StatusHint = Format(T("DevicesMacLockedFormat"), ClaimedBluetoothMac!);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
            await RunOnMainThreadAsync(RefreshItems).ConfigureAwait(false);
            RaiseCanExecuteChanged(ScanCommand, StopScanCommand, ConnectCommand, DisconnectCommand);
        }
    }

    private async Task DisconnectAsync()
    {
        IsBusy = true;
        try
        {
            await _ble.DisconnectAsync().ConfigureAwait(false);
            OnPropertyChanged(nameof(ConnectedDeviceId));
            OnPropertyChanged(nameof(IsBleConnected));
            OnPropertyChanged(nameof(ShowVitalsWaiting));
            OnPropertyChanged(nameof(ShowReadingsEmpty));
            LastVitals = null;
            _lastHeartAt = null;
            _lastSpo2At = null;
            SyncResultText = null;
            StatusHint = T("DevicesDisconnected");
        }
        finally
        {
            IsBusy = false;
            await RunOnMainThreadAsync(RefreshItems).ConfigureAwait(false);
            RaiseCanExecuteChanged(ScanCommand, StopScanCommand, ConnectCommand, DisconnectCommand);
        }
    }

    private async Task ScanPackagingAsync()
    {
        ErrorMessage = null;
        SyncResultText = null;

        var camera = await Permissions.RequestAsync<Permissions.Camera>().ConfigureAwait(false);
        if (camera != PermissionStatus.Granted)
        {
            ErrorMessage = T("DevicesScanPackagingCameraDenied");
            return;
        }

        FileResult? photo;
        try
        {
            photo = await MainThread.InvokeOnMainThreadAsync(() =>
                MediaPicker.Default.CapturePhotoAsync()).ConfigureAwait(false);
        }
        catch (FeatureNotSupportedException)
        {
            ErrorMessage = T("DevicesScanPackagingUnsupported");
            return;
        }
        catch
        {
            ErrorMessage = T("DevicesScanPackagingFailed");
            return;
        }

        if (photo is null)
            return;

        IsBusy = true;
        try
        {
            await using var stream = await photo.OpenReadAsync().ConfigureAwait(false);
            var payload = PackagingBarcodeDecoder.Decode(stream);
            if (string.IsNullOrWhiteSpace(payload))
            {
                ErrorMessage = T("DevicesScanPackagingNoCode");
                return;
            }

            var parsed = FleetDeviceScanParser.ParseBarcode(payload);
            if (string.IsNullOrWhiteSpace(parsed.PreferredValue) || parsed.PreferredLooksLikeMac)
            {
                ErrorMessage = T("DevicesScanPackagingNeedSerial");
                return;
            }

            SerialNumber = parsed.PreferredValue;
            if (!string.IsNullOrWhiteSpace(parsed.SuggestedModel))
            {
                var match = ModelSkuOptions.FirstOrDefault(o =>
                    o.Equals(parsed.SuggestedModel, StringComparison.OrdinalIgnoreCase));
                if (match is not null)
                    ModelSku = match;
            }

            SyncResultText = Format(T("DevicesScanPackagingFilledFormat"), SerialNumber);
        }
        finally
        {
            IsBusy = false;
            RaiseCanExecuteChanged(RegisterCommand, ScanPackagingCommand, SyncLastReadingCommand);
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
                ErrorMessage = T("DevicesClaimSerialRequired");
                return;
            }

            var sku = (ModelSku ?? "").Trim();
            if (string.IsNullOrWhiteSpace(sku))
            {
                ErrorMessage = T("DevicesClaimSkuRequired");
                return;
            }

            var resp = await _patientDevices.RegisterMyDeviceAsync(serial, sku).ConfigureAwait(false);
            if (!resp.IsSuccess)
            {
                ErrorMessage = resp.ErrorMessage ?? T("DevicesClaimFailed");
                return;
            }

            RegisteredDeviceId = resp.Data?.DeviceId;
            ClaimedBluetoothMac = BluetoothMacNormalizer.TryNormalize(resp.Data?.BluetoothMacAddress);
            OnPropertyChanged(nameof(ClaimedDeviceIdLabel));
            SyncResultText = RegisteredDeviceId is { } id
                ? Format(T("DevicesClaimSuccessFormat"), id)
                : T("DevicesClaimSuccess");
            StatusHint = string.IsNullOrWhiteSpace(ClaimedBluetoothMac)
                ? T("DevicesClaimReadyFirstPairHint")
                : Format(T("DevicesClaimReadyMacHint"), ClaimedBluetoothMac!);
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
                ErrorMessage = T("DevicesClaimBeforeSync");
                return;
            }

            var last = LastVitals;
            var hasHr = last?.HeartRateBpm is int;
            var hasSpo2 = last?.SpO2Percent is decimal;

            if (!hasHr && !hasSpo2)
            {
                ErrorMessage = "No heart rate or SpO₂ yet. Connect and wait for BLE notifications.";
                return;
            }

            var hrList = new List<HeartRateReadingInput>();
            if (hasHr && _lastHeartAt is { } hrAt && last!.HeartRateBpm is int bpm)
                hrList.Add(new HeartRateReadingInput { RecordedAt = hrAt.UtcDateTime, BeatsPerMinute = bpm });

            var spo2List = new List<Spo2ReadingInput>();
            if (hasSpo2 && _lastSpo2At is { } spo2At && last!.SpO2Percent is decimal spo2Pct)
            {
                spo2List.Add(new Spo2ReadingInput
                {
                    RecordedAt = spo2At.UtcDateTime,
                    SpO2 = spo2Pct,
                    PulseRate = last.SpO2PulseBpm.HasValue ? (decimal)last.SpO2PulseBpm.Value : null
                });
            }

            var resp = await _patientDevices.SyncReadingsAsync(
                deviceId,
                hrList,
                spo2List,
                CancellationToken.None).ConfigureAwait(false);

            if (!resp.IsSuccess)
            {
                ErrorMessage = resp.ErrorMessage ?? "Sync failed.";
                var code = resp.StatusCode;
                if (code is null or >= 500 or 408 or 429)
                {
                    await _vitalsOutbox
                        .EnqueueAsync(deviceId, hrList, spo2List, CancellationToken.None)
                        .ConfigureAwait(false);
                    StatusHint = T("DevicesSyncQueuedOfflineHint");
                }

                return;
            }

            var dto = resp.Data;
            SyncResultText = dto is null
                ? "Synced."
                : $"Synced HR={dto.HeartRateCount}, SpO₂={dto.SpO2Count}";

            try
            {
                var flushed = await _vitalsOutbox.TryFlushAsync(_patientDevices, CancellationToken.None).ConfigureAwait(false);
                if (flushed > 0)
                    SyncResultText += " " + Format(T("DevicesOutboxFlushedFormat"), flushed);
            }
            catch
            {
                // ignore secondary flush errors
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
