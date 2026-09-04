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

public sealed class HealthRecordDetailViewModel : BaseViewModel
{
    private readonly IHealthRecordService _healthRecords;
    private Guid _visitId;
    private string _when = string.Empty;
    private string _status = string.Empty;
    private string _visitType = string.Empty;
    private string _summary = string.Empty;
    private string? _errorMessage;

    public HealthRecordDetailViewModel(IHealthRecordService healthRecords)
    {
        _healthRecords = healthRecords ?? throw new ArgumentNullException(nameof(healthRecords));
        Title = T("RecordsDetailTitle");
        StatusLabel = T("RecordsStatus");
        TypeLabel = T("RecordsVisitType");
        SummaryLabel = T("RecordsSummary");
        WhenLabel = T("RecordsWhen");
        VitalsSectionTitle = T("RecordsVitalsSection");
        PrescriptionsSectionTitle = T("RecordsPrescriptionsSection");
        LabsSectionTitle = T("RecordsLabsSection");
        BackCommand = new Command(async () => await SafeShellNavigator.GoToAsync(".."));
        VitalSigns = new ObservableCollection<VitalSignDisplayItem>();
        Prescriptions = new ObservableCollection<CollectionOrderDisplayItem>();
        LabOrders = new ObservableCollection<CollectionOrderDisplayItem>();
    }

    public string WhenLabel { get; }
    public string StatusLabel { get; }
    public string TypeLabel { get; }
    public string SummaryLabel { get; }
    public string VitalsSectionTitle { get; }
    public string PrescriptionsSectionTitle { get; }
    public string LabsSectionTitle { get; }

    public string WhenText
    {
        get => _when;
        set => SetProperty(ref _when, value);
    }

    public string StatusText
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string VisitTypeText
    {
        get => _visitType;
        set => SetProperty(ref _visitType, value);
    }

    public string SummaryText
    {
        get => _summary;
        set => SetProperty(ref _summary, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ObservableCollection<VitalSignDisplayItem> VitalSigns { get; }
    public ObservableCollection<CollectionOrderDisplayItem> Prescriptions { get; }
    public ObservableCollection<CollectionOrderDisplayItem> LabOrders { get; }

    public ICommand BackCommand { get; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!TryGetQueryGuid(query, "visitId", out var id))
            return;
        _visitId = id;
    }

    public async Task LoadAsync()
    {
        if (_visitId == Guid.Empty)
        {
            ErrorMessage = T("RecordsDetailFailed");
            return;
        }

        ErrorMessage = null;
        IsBusy = true;
        try
        {
            await RunOnMainThreadAsync(() =>
            {
                VitalSigns.Clear();
                Prescriptions.Clear();
                LabOrders.Clear();
            }).ConfigureAwait(false);

            var response = await _healthRecords.GetMyHealthRecordAsync(_visitId, CancellationToken.None).ConfigureAwait(false);
            if (!response.IsSuccess || response.Data is null)
            {
                ErrorMessage = response.ErrorMessage ?? T("RecordsDetailFailed");
                return;
            }

            var r = response.Data;
            var culture = CultureInfo.CurrentCulture;
            var start = r.VisitStart.ToLocalTime();
            var end = r.VisitEnd?.ToLocalTime();
            var whenText = end is null
                ? start.ToString("F", culture)
                : $"{start.ToString("F", culture)} to {end.Value.ToString("t", culture)}";
            var statusText = r.Status;
            var visitTypeText = r.VisitType;
            var summaryText = string.IsNullOrWhiteSpace(r.Summary) ? "—" : r.Summary!;

            var vitals = new List<VitalSignDisplayItem>();
            foreach (var v in r.VitalSigns)
            {
                var unit = string.IsNullOrWhiteSpace(v.Unit) ? string.Empty : $" {v.Unit}";
                vitals.Add(new VitalSignDisplayItem
                {
                    MainLine = $"{v.Type} {v.Value}{unit}".Trim(),
                    RecordedLine = v.RecordedAt.ToLocalTime().ToString("g", culture)
                });
            }

            var prescriptions = new List<CollectionOrderDisplayItem>();
            foreach (var rx in r.Prescriptions)
            {
                var meds = rx.Items.Count == 0
                    ? T("RecordsPickupPrescription")
                    : string.Join(", ", rx.Items.Select(i => i.MedicationName));
                prescriptions.Add(new CollectionOrderDisplayItem
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
                });
            }

            var labOrders = new List<CollectionOrderDisplayItem>();
            foreach (var lab in r.LabOrders)
            {
                labOrders.Add(new CollectionOrderDisplayItem
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
                });
            }

            await RunOnMainThreadAsync(() =>
            {
                WhenText = whenText;
                StatusText = statusText;
                VisitTypeText = visitTypeText;
                SummaryText = summaryText;

                VitalSigns.Clear();
                foreach (var item in vitals)
                    VitalSigns.Add(item);

                Prescriptions.Clear();
                foreach (var item in prescriptions)
                    Prescriptions.Add(item);

                LabOrders.Clear();
                foreach (var item in labOrders)
                    LabOrders.Add(item);
            }).ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
        }
    }

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
}
