using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using RaphCare.Client;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.HealthRecords;
using RaphCare.Mobile.Core.Features.Records.Models;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Records.ViewModels;

public sealed class RecordsViewModel : BaseViewModel
{
    private readonly IHealthRecordService _healthRecords;
    private readonly CollectionCheckInStore _checkIn;
    private readonly List<CollectionOrderDisplayItem> _allPickup = [];
    private string? _errorMessage;
    private string _pickupHint;
    private string _scanButtonText;

    public RecordsViewModel(IHealthRecordService healthRecords, CollectionCheckInStore checkIn)
    {
        _healthRecords = healthRecords ?? throw new ArgumentNullException(nameof(healthRecords));
        _checkIn = checkIn ?? throw new ArgumentNullException(nameof(checkIn));
        Title = T("RecordsListTitle");
        RefreshButtonText = T("RecordsRefresh");
        EmptyStateText = T("RecordsEmpty");
        PickupSectionTitle = T("RecordsPickupSection");
        _pickupHint = T("RecordsPickupHint");
        _scanButtonText = T("RecordsScanPoster");
        ClearCheckInButtonText = T("RecordsClearCheckIn");

        RefreshCommand = new Command(async () => await LoadAsync());
        ScanPosterCommand = new Command(async () => await ScanPosterAsync());
        ClearCheckInCommand = new Command(() => _checkIn.SetClinic(null));
        OpenDetailCommand = new Command<Guid>(async id =>
        {
            if (id == Guid.Empty)
                return;
            await SafeShellNavigator.GoToAsync($"{AppNavigator.HealthRecordDetail}?visitId={id}");
        });

        Items.CollectionChanged += (_, _) => NotifyListChanged();
        PickupItems.CollectionChanged += (_, _) => NotifyListChanged();
    }

    public ObservableCollection<HealthRecordListDisplayItem> Items { get; } = new();
    public ObservableCollection<CollectionOrderDisplayItem> PickupItems { get; } = new();

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool ShowEmpty => !IsBusy && Items.Count == 0 && PickupItems.Count == 0 && string.IsNullOrEmpty(ErrorMessage);
    public bool ShowPickup => PickupItems.Count > 0;
    public bool ShowClearCheckIn => _checkIn.ClinicId is not null;

    public string RefreshButtonText { get; }
    public string EmptyStateText { get; }
    public string PickupSectionTitle { get; }
    public string ClearCheckInButtonText { get; }

    public string PickupHint
    {
        get => _pickupHint;
        private set => SetProperty(ref _pickupHint, value);
    }

    public string ScanButtonText
    {
        get => _scanButtonText;
        private set => SetProperty(ref _scanButtonText, value);
    }

    public ICommand RefreshCommand { get; }
    public ICommand ScanPosterCommand { get; }
    public ICommand ClearCheckInCommand { get; }
    public ICommand OpenDetailCommand { get; }

    public void Attach()
    {
        _checkIn.Changed -= OnCheckInChanged;
        _checkIn.Changed += OnCheckInChanged;
        ApplyCheckInFilter();
    }

    public void Detach() => _checkIn.Changed -= OnCheckInChanged;

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        ErrorMessage = null;
        IsBusy = true;
        NotifyListChanged();
        try
        {
            var recordsTask = _healthRecords.GetMyHealthRecordsAsync(1, 50, CancellationToken.None);
            var pickupTask = _healthRecords.GetMyCollectionOrdersAsync(CancellationToken.None);
            await Task.WhenAll(recordsTask, pickupTask).ConfigureAwait(false);

            var records = recordsTask.Result;
            if (!records.IsSuccess || records.Data is null)
            {
                ErrorMessage = records.ErrorMessage ?? T("RecordsLoadFailed");
                Items.Clear();
            }
            else
            {
                Items.Clear();
                var culture = CultureInfo.CurrentCulture;
                foreach (var r in records.Data.Items)
                    Items.Add(MapItem(r, culture));
            }

            _allPickup.Clear();
            var pickup = pickupTask.Result;
            if (pickup.IsSuccess && pickup.Data is not null)
            {
                foreach (var rx in pickup.Data.Prescriptions)
                    _allPickup.Add(MapPrescription(rx));
                foreach (var lab in pickup.Data.LabOrders)
                    _allPickup.Add(MapLab(lab));
            }

            ApplyCheckInFilter();
        }
        finally
        {
            IsBusy = false;
            NotifyListChanged();
        }
    }

    private void OnCheckInChanged() =>
        MainThread.BeginInvokeOnMainThread(ApplyCheckInFilter);

    private void ApplyCheckInFilter()
    {
        PickupItems.Clear();
        var clinicId = _checkIn.ClinicId;
        IEnumerable<CollectionOrderDisplayItem> items = _allPickup;
        if (clinicId is { } filter)
        {
            items = _allPickup.Where(i => i.ClinicId == filter);
            PickupHint = items.Any(i => i.IsCalled)
                ? T("RecordsPickupCalledHint")
                : T("RecordsPickupCheckInHint");
        }
        else
        {
            PickupHint = _allPickup.Any(i => i.IsCalled)
                ? T("RecordsPickupCalledHint")
                : T("RecordsPickupHint");
        }

        foreach (var item in items.OrderByDescending(i => i.IsCalled))
            PickupItems.Add(item);

        OnPropertyChanged(nameof(ShowClearCheckIn));
        NotifyListChanged();
    }

    private async Task ScanPosterAsync()
    {
        ErrorMessage = null;
        var camera = await Permissions.RequestAsync<Permissions.Camera>().ConfigureAwait(false);
        if (camera != PermissionStatus.Granted)
        {
            ErrorMessage = T("RecordsScanCameraDenied");
            return;
        }

        FileResult? photo;
        try
        {
            photo = await MediaPicker.Default.CapturePhotoAsync().ConfigureAwait(false);
        }
        catch
        {
            ErrorMessage = T("RecordsScanFailed");
            return;
        }

        if (photo is null)
            return;

        await using var stream = await photo.OpenReadAsync().ConfigureAwait(false);
        var text = QrImageDecoder.Decode(stream);
        if (string.IsNullOrWhiteSpace(text))
        {
            ErrorMessage = T("RecordsScanFailed");
            return;
        }

        if (CollectionQr.TryParsePoster(text, out var clinicId))
        {
            _checkIn.SetClinic(clinicId);
            return;
        }

        ErrorMessage = T("RecordsScanNotPoster");
    }

    private void NotifyListChanged()
    {
        OnPropertyChanged(nameof(ShowEmpty));
        OnPropertyChanged(nameof(ShowPickup));
        OnPropertyChanged(nameof(ShowClearCheckIn));
    }

    private static CollectionOrderDisplayItem MapPrescription(PatientCollectionPrescriptionViewModel rx)
    {
        var meds = rx.Items.Count == 0
            ? T("RecordsPickupPrescription")
            : string.Join(", ", rx.Items.Select(i => i.MedicationName));
        return new CollectionOrderDisplayItem
        {
            VisitId = rx.VisitId,
            ClinicId = rx.ClinicId,
            KindLabel = T("RecordsPickupPrescription"),
            ClinicName = rx.ClinicName,
            PickupCode = rx.PickupCode,
            DetailLine = meds,
            StatusText = FormatOrderStatus(rx.Status, rx.CalledAt),
            IsCalled = rx.CalledAt is not null && rx.Status == "Pending",
            QrImage = QrImage(rx.PickupCode)
        };
    }

    private static CollectionOrderDisplayItem MapLab(PatientCollectionLabOrderViewModel lab) => new()
    {
        VisitId = lab.VisitId,
        ClinicId = lab.ClinicId,
        KindLabel = T("RecordsPickupLab"),
        ClinicName = lab.ClinicName,
        PickupCode = lab.PickupCode,
        DetailLine = lab.TestName,
        StatusText = FormatOrderStatus(lab.Status, lab.CalledAt),
        IsCalled = lab.CalledAt is not null && lab.Status == "Pending",
        QrImage = QrImage(lab.PickupCode)
    };

    private static ImageSource? QrImage(string pickupCode)
    {
        if (string.IsNullOrWhiteSpace(pickupCode))
            return null;
        var bytes = PickupQr.ToPng(pickupCode);
        return ImageSource.FromStream(() => new MemoryStream(bytes));
    }

    private static string FormatOrderStatus(string? status, DateTime? calledAt) =>
        status switch
        {
            "Dispensed" => T("RecordsPickupCollected"),
            "Completed" => T("RecordsPickupCompleted"),
            "Pending" when calledAt is not null => T("RecordsPickupCalled"),
            "Pending" => T("RecordsPickupWaiting"),
            "Cancelled" => T("RecordsPickupCancelled"),
            _ => string.IsNullOrWhiteSpace(status) ? string.Empty : status
        };

    private static HealthRecordListDisplayItem MapItem(HealthRecordListItemViewModel r, CultureInfo culture)
    {
        var start = r.VisitStart.ToLocalTime();
        var end = r.VisitEnd?.ToLocalTime();
        var when = end is null
            ? start.ToString("g", culture)
            : $"{start.ToString("g", culture)} to {end.Value.ToString("t", culture)}";
        var secondary = string.IsNullOrWhiteSpace(r.Summary)
            ? r.Status
            : $"{r.Status} · {r.Summary}";
        return new HealthRecordListDisplayItem
        {
            Id = r.Id,
            PrimaryLine = $"{when} · {r.VisitType}",
            SecondaryLine = secondary
        };
    }
}
