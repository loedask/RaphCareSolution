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
    private string? _errorMessage;

    public RecordsViewModel(IHealthRecordService healthRecords)
    {
        _healthRecords = healthRecords ?? throw new ArgumentNullException(nameof(healthRecords));
        Title = T("RecordsListTitle");
        RefreshButtonText = T("RecordsRefresh");
        EmptyStateText = T("RecordsEmpty");
        PickupSectionTitle = T("RecordsPickupSection");
        PickupHint = T("RecordsPickupHint");

        RefreshCommand = new Command(async () => await LoadAsync());
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

    public string RefreshButtonText { get; }
    public string EmptyStateText { get; }
    public string PickupSectionTitle { get; }
    public string PickupHint { get; }

    public ICommand RefreshCommand { get; }
    public ICommand OpenDetailCommand { get; }

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

            PickupItems.Clear();
            var pickup = pickupTask.Result;
            if (pickup.IsSuccess && pickup.Data is not null)
            {
                foreach (var rx in pickup.Data.Prescriptions)
                    PickupItems.Add(MapPrescription(rx));
                foreach (var lab in pickup.Data.LabOrders)
                    PickupItems.Add(MapLab(lab));
            }
        }
        finally
        {
            IsBusy = false;
            NotifyListChanged();
        }
    }

    private void NotifyListChanged()
    {
        OnPropertyChanged(nameof(ShowEmpty));
        OnPropertyChanged(nameof(ShowPickup));
    }

    private static CollectionOrderDisplayItem MapPrescription(PatientCollectionPrescriptionViewModel rx)
    {
        var meds = rx.Items.Count == 0
            ? T("RecordsPickupPrescription")
            : string.Join(", ", rx.Items.Select(i => i.MedicationName));
        return new CollectionOrderDisplayItem
        {
            VisitId = rx.VisitId,
            KindLabel = T("RecordsPickupPrescription"),
            ClinicName = rx.ClinicName,
            PickupCode = rx.PickupCode,
            DetailLine = meds,
            StatusText = FormatOrderStatus(rx.Status),
            QrImage = QrImage(rx.PickupCode)
        };
    }

    private static CollectionOrderDisplayItem MapLab(PatientCollectionLabOrderViewModel lab) => new()
    {
        VisitId = lab.VisitId,
        KindLabel = T("RecordsPickupLab"),
        ClinicName = lab.ClinicName,
        PickupCode = lab.PickupCode,
        DetailLine = lab.TestName,
        StatusText = FormatOrderStatus(lab.Status),
        QrImage = QrImage(lab.PickupCode)
    };

    private static ImageSource? QrImage(string pickupCode)
    {
        if (string.IsNullOrWhiteSpace(pickupCode))
            return null;
        var bytes = PickupQr.ToPng(pickupCode);
        return ImageSource.FromStream(() => new MemoryStream(bytes));
    }

    private static string FormatOrderStatus(string? status) =>
        status switch
        {
            "Dispensed" => T("RecordsPickupCollected"),
            "Completed" => T("RecordsPickupCompleted"),
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
