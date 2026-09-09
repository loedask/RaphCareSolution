using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Devices;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;
using RaphCare.Mobile.Core.Features.Devices.Models;
using RaphCare.Mobile.Core.Features.Devices.Services;
using RaphCare.Mobile.Kernel.Core.Common.Devices;

namespace RaphCare.Mobile.Core.Features.Devices.ViewModels;

/// <summary>Live and synced watch readings (HR, SpO₂ today; room for more measures later).</summary>
public sealed class WatchReadingsViewModel : BaseViewModel, IDisposable
{
    private readonly IWearableBleCoordinator _ble;
    private readonly IPatientDevicesService _patientDevices;
    private readonly IVitalsSyncOutbox _vitalsOutbox;
    private string? _errorMessage;
    private string? _statusHint;
    private string? _syncResultText;
    private WearableVitalsSnapshot? _lastVitals;
    private Guid? _registeredDeviceId;
    private bool _isMeasuring;

    public WatchReadingsViewModel(
        IWearableBleCoordinator ble,
        IPatientDevicesService patientDevices,
        IVitalsSyncOutbox vitalsOutbox)
    {
        _ble = ble ?? throw new ArgumentNullException(nameof(ble));
        _patientDevices = patientDevices ?? throw new ArgumentNullException(nameof(patientDevices));
        _vitalsOutbox = vitalsOutbox ?? throw new ArgumentNullException(nameof(vitalsOutbox));
        Title = T("WatchReadingsPageTitle");
        PageHint = T("WatchReadingsPageHint");
        HeartRateLabel = T("WatchReadingsHeartRateLabel");
        Spo2Label = T("WatchReadingsSpo2Label");
        MeasureButtonText = T("WatchReadingsMeasureButton");
        SyncButtonText = T("WatchReadingsSyncButton");
        ComingSoonTitle = T("WatchReadingsComingSoonTitle");
        ComingSoonBody = T("WatchReadingsComingSoonBody");
        NotConnectedHint = T("WatchReadingsNotConnectedHint");

        BackCommand = new Command(async () => await SafeShellNavigator.GoToAsync(".."));
        MeasureCommand = new Command(async () => await MeasureAsync().ConfigureAwait(false), () => CanMeasure);
        SyncCommand = new Command(async () => await SyncAsync().ConfigureAwait(false), () => CanSync);

        _ble.VitalsUpdated += OnVitalsUpdated;
        _ble.ErrorOccurred += OnBleError;
        ApplySnapshot(_ble.LastVitals);
    }

    public string PageHint { get; }
    public string HeartRateLabel { get; }
    public string Spo2Label { get; }
    public string MeasureButtonText { get; }
    public string SyncButtonText { get; }
    public string ComingSoonTitle { get; }
    public string ComingSoonBody { get; }
    public string NotConnectedHint { get; }

    public ICommand BackCommand { get; }
    public ICommand MeasureCommand { get; }
    public ICommand SyncCommand { get; }

    public bool IsBleConnected => _ble.ConnectedDeviceId.HasValue;
    public bool ShowNotConnected => !IsBleConnected;

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

    public string? SyncResultText
    {
        get => _syncResultText;
        private set => SetProperty(ref _syncResultText, value);
    }

    public bool IsMeasuring
    {
        get => _isMeasuring;
        private set
        {
            if (!SetProperty(ref _isMeasuring, value))
                return;
            RaiseCanExecuteChanged(MeasureCommand, SyncCommand);
        }
    }

    public string? HeartRateValue =>
        _lastVitals?.HeartRateBpm is int bpm
            ? Format(T("DevicesHeartRateFormat"), bpm)
            : T("WatchReadingsValueEmpty");

    public string? Spo2Value =>
        _lastVitals?.SpO2Percent is decimal spo2
            ? Format(T("DevicesSpO2Format"), spo2)
            : T("WatchReadingsValueEmpty");

    public bool CanMeasure =>
        !IsBusy && !IsMeasuring && IsBleConnected;

    public bool CanSync =>
        !IsBusy
        && !IsMeasuring
        && _registeredDeviceId.HasValue
        && DevicesVitalsDisplayPolicy.HasPatientFacingReading(_lastVitals?.HeartRateBpm, _lastVitals?.SpO2Percent);

    public void AttachBleHandlers()
    {
        DetachBleHandlers();
        _ble.VitalsUpdated += OnVitalsUpdated;
        _ble.ErrorOccurred += OnBleError;
    }

    public void DetachBleHandlers()
    {
        _ble.VitalsUpdated -= OnVitalsUpdated;
        _ble.ErrorOccurred -= OnBleError;
    }

    public void Dispose()
    {
        DetachBleHandlers();
        GC.SuppressFinalize(this);
    }

    public async Task OnAppearingAsync()
    {
        OnPropertyChanged(nameof(IsBleConnected));
        OnPropertyChanged(nameof(ShowNotConnected));
        RaiseCanExecuteChanged(MeasureCommand, SyncCommand);
        ApplySnapshot(_ble.LastVitals);

        try
        {
            var devices = await _patientDevices.GetMyDevicesAsync(CancellationToken.None).ConfigureAwait(false);
            if (devices.IsSuccess && devices.Data is { Count: > 0 })
            {
                var first = devices.Data[0];
                _registeredDeviceId = first.DeviceId;
                var mac = BluetoothMacNormalizer.TryNormalize(first.BluetoothMacAddress);
                var perm = await _ble.CheckBluetoothPermissionsAsync().ConfigureAwait(false);
                if (DevicesBleSessionPolicy.ShouldReconnectClaimedWatchOnAppearWithPermission(
                        nearbyDevicesPermissionGranted: perm == PermissionStatus.Granted,
                        hasLockedBluetoothMac: mac is not null,
                        hasConnectedDeviceId: _ble.ConnectedDeviceId.HasValue,
                        liveMeasureSessionReady: _ble.IsLiveMeasureSessionReady))
                {
                    StatusHint = T("DevicesReconnectingHint");
                    await _ble.ReconnectClaimedWatchAsync(
                            mac!,
                            string.IsNullOrWhiteSpace(first.Model) ? null : first.Model,
                            CancellationToken.None)
                        .ConfigureAwait(false);
                    OnPropertyChanged(nameof(IsBleConnected));
                    OnPropertyChanged(nameof(ShowNotConnected));
                    if (!_ble.ConnectedDeviceId.HasValue)
                        StatusHint = NotConnectedHint;
                }
                else if (mac is not null
                         && perm != PermissionStatus.Granted
                         && !DevicesBleSessionPolicy.ShouldRequestBluetoothPermissionOnDevicesAppear)
                {
                    StatusHint = T("DevicesPermissionNeededHint");
                }
            }

            var latest = await _patientDevices.GetMyLatestReadingsAsync(CancellationToken.None).ConfigureAwait(false);
            if (latest.IsSuccess && latest.Data is not null)
                MergeFromServer(latest.Data);

            if (!IsBleConnected)
                StatusHint = NotConnectedHint;
            else if (!_ble.IsLiveMeasureSessionReady)
                StatusHint = T("WatchReadingsNeedsDevicesConnectHint");
            else if (!DevicesBleSessionPolicy.EnableVendorLiveMeasure)
                StatusHint = T("WatchReadingsVendorMeasureDisabledHint");
            else if (!DevicesVitalsDisplayPolicy.HasPatientFacingReading(_lastVitals?.HeartRateBpm, _lastVitals?.SpO2Percent))
                StatusHint = T("WatchReadingsReadyExclusiveHint");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            RaiseCanExecuteChanged(MeasureCommand, SyncCommand);
        }
    }

    private void MergeFromServer(PatientLatestReadingsViewModel data)
    {
        if (_lastVitals?.HeartRateBpm is not null && _lastVitals?.SpO2Percent is not null)
            return;

        var hr = data.HeartRateBpm is decimal d ? (int?)decimal.ToInt32(d) : null;
        var spo2 = data.SpO2Percent;
        if (hr is null && spo2 is null)
            return;

        ApplySnapshot(new WearableVitalsSnapshot
        {
            At = data.HeartRateRecordedAt ?? data.SpO2RecordedAt ?? DateTimeOffset.UtcNow,
            HeartRateBpm = hr ?? _lastVitals?.HeartRateBpm,
            SpO2Percent = spo2 ?? _lastVitals?.SpO2Percent,
            CharacteristicUuid = "api:latest",
        });
    }

    private void ApplySnapshot(WearableVitalsSnapshot? snap)
    {
        _lastVitals = snap;
        OnPropertyChanged(nameof(HeartRateValue));
        OnPropertyChanged(nameof(Spo2Value));
        RaiseCanExecuteChanged(SyncCommand);
    }

    private void OnVitalsUpdated(object? sender, WearableVitalsSnapshot e) =>
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ApplySnapshot(e);
            StatusHint = T("WatchReadingsUpdatedHint");
            ErrorMessage = null;
        });

    private void OnBleError(object? sender, string? message) =>
        MainThread.BeginInvokeOnMainThread(() => ErrorMessage = message);

    private async Task MeasureAsync()
    {
        ErrorMessage = null;
        SyncResultText = null;
        if (!IsBleConnected)
        {
            ErrorMessage = NotConnectedHint;
            return;
        }

        IsMeasuring = true;
        StatusHint = T("WatchReadingsMeasuringHint");
        try
        {
            using var cts = new CancellationTokenSource(DevicesBleSessionPolicy.LiveMeasureOverallTimeout);
            var snap = await _ble.MeasureLiveVitalsAsync(cts.Token).ConfigureAwait(false);
            if (snap is not null
                && DevicesVitalsDisplayPolicy.HasPatientFacingReading(snap.HeartRateBpm, snap.SpO2Percent))
            {
                ApplySnapshot(snap);
                StatusHint = T("WatchReadingsUpdatedHint");
            }
            else if (!DevicesBleSessionPolicy.EnableVendorLiveMeasure)
            {
                ErrorMessage = T("WatchReadingsVendorMeasureDisabled");
                StatusHint = T("WatchReadingsVendorMeasureDisabledHint");
            }
            else if (!string.IsNullOrWhiteSpace(ErrorMessage))
            {
                StatusHint = T("WatchReadingsMeasureFailedHint");
            }
            else
            {
                StatusHint = T("WatchReadingsMeasureFailedHint");
            }
        }
        catch (OperationCanceledException)
        {
            ErrorMessage = T("WatchReadingsMeasureTimeout");
            StatusHint = T("WatchReadingsMeasureFailedHint");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            StatusHint = T("WatchReadingsMeasureFailedHint");
        }
        finally
        {
            IsMeasuring = false;
            OnPropertyChanged(nameof(IsBleConnected));
            OnPropertyChanged(nameof(ShowNotConnected));
            RaiseCanExecuteChanged(MeasureCommand, SyncCommand);
        }
    }

    private async Task SyncAsync()
    {
        ErrorMessage = null;
        SyncResultText = null;
        if (_registeredDeviceId is not Guid deviceId
            || !DevicesVitalsDisplayPolicy.HasPatientFacingReading(_lastVitals?.HeartRateBpm, _lastVitals?.SpO2Percent)
            || _lastVitals is null)
        {
            ErrorMessage = T("WatchReadingsSyncNoVitals");
            return;
        }

        IsBusy = true;
        try
        {
            var at = _lastVitals.At.UtcDateTime;
            var hrList = new List<HeartRateReadingInput>();
            if (_lastVitals.HeartRateBpm is int bpm)
                hrList.Add(new HeartRateReadingInput { RecordedAt = at, BeatsPerMinute = bpm });

            var spo2List = new List<Spo2ReadingInput>();
            if (_lastVitals.SpO2Percent is decimal sp)
            {
                spo2List.Add(new Spo2ReadingInput
                {
                    RecordedAt = at,
                    SpO2 = sp,
                    PulseRate = _lastVitals.SpO2PulseBpm.HasValue ? _lastVitals.SpO2PulseBpm.Value : null,
                });
            }

            var resp = await _patientDevices.SyncReadingsAsync(deviceId, hrList, spo2List, CancellationToken.None)
                .ConfigureAwait(false);
            if (!resp.IsSuccess)
            {
                await _vitalsOutbox.EnqueueAsync(deviceId, hrList, spo2List, CancellationToken.None).ConfigureAwait(false);
                SyncResultText = resp.ErrorMessage ?? T("DevicesSyncQueuedOfflineHint");
                return;
            }

            var dto = resp.Data;
            SyncResultText = dto is null
                ? T("WatchReadingsSyncOk")
                : Format(T("WatchReadingsSyncOkFormat"), dto.HeartRateCount, dto.SpO2Count);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
            RaiseCanExecuteChanged(MeasureCommand, SyncCommand);
        }
    }
}
