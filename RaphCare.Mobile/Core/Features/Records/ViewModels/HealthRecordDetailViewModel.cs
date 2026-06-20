using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
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
        BackCommand = new Command(async () => await SafeShellNavigator.GoToAsync(".."));
        VitalSigns = new ObservableCollection<VitalSignDisplayItem>();
    }

    public string WhenLabel { get; }
    public string StatusLabel { get; }
    public string TypeLabel { get; }
    public string SummaryLabel { get; }
    public string VitalsSectionTitle { get; }

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
        VitalSigns.Clear();
        try
        {
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
            WhenText = end is null
                ? start.ToString("F", culture)
                : $"{start.ToString("F", culture)} – {end.Value.ToString("t", culture)}";
            StatusText = r.Status;
            VisitTypeText = r.VisitType;
            SummaryText = string.IsNullOrWhiteSpace(r.Summary) ? "—" : r.Summary!;

            foreach (var v in r.VitalSigns)
            {
                var unit = string.IsNullOrWhiteSpace(v.Unit) ? string.Empty : $" {v.Unit}";
                VitalSigns.Add(new VitalSignDisplayItem
                {
                    MainLine = $"{v.Type} {v.Value}{unit}".Trim(),
                    RecordedLine = v.RecordedAt.ToLocalTime().ToString("g", culture)
                });
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
