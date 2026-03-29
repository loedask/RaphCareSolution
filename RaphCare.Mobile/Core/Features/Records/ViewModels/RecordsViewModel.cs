using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.HealthRecords;
using RaphCare.Mobile.Core.Features.Records.Models;
using RaphCare.Mobile.Core.Shared.Navigation;
using RaphCare.Mobile.Core.Shared.ViewModels;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Features.Records.ViewModels;

public sealed class RecordsViewModel : BaseViewModel
{
    private readonly IHealthRecordService _healthRecords;
    private string? _errorMessage;

    public RecordsViewModel(IHealthRecordService healthRecords)
    {
        _healthRecords = healthRecords ?? throw new ArgumentNullException(nameof(healthRecords));
        Title = AppResources.T("RecordsListTitle");
        RefreshButtonText = AppResources.T("RecordsRefresh");
        EmptyStateText = AppResources.T("RecordsEmpty");

        RefreshCommand = new Command(async () => await LoadAsync());
        OpenDetailCommand = new Command<Guid>(async id => await SafeShellNavigator.GoToAsync($"{AppNavigator.HealthRecordDetail}?visitId={id}"));

        Items.CollectionChanged += (_, _) => NotifyEmptyChanged();
    }

    public ObservableCollection<HealthRecordListDisplayItem> Items { get; } = new();

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool ShowEmpty => !IsBusy && Items.Count == 0 && string.IsNullOrEmpty(ErrorMessage);

    public string RefreshButtonText { get; }
    public string EmptyStateText { get; }

    public ICommand RefreshCommand { get; }
    public ICommand OpenDetailCommand { get; }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        ErrorMessage = null;
        IsBusy = true;
        NotifyEmptyChanged();
        try
        {
            var response = await _healthRecords.GetMyHealthRecordsAsync(1, 50, CancellationToken.None).ConfigureAwait(false);
            if (!response.IsSuccess || response.Data is null)
            {
                ErrorMessage = response.ErrorMessage ?? AppResources.T("RecordsLoadFailed");
                Items.Clear();
                NotifyEmptyChanged();
                return;
            }

            Items.Clear();
            var culture = CultureInfo.CurrentCulture;
            foreach (var r in response.Data.Items)
                Items.Add(MapItem(r, culture));
        }
        finally
        {
            IsBusy = false;
            NotifyEmptyChanged();
        }
    }

    private void NotifyEmptyChanged() => OnPropertyChanged(nameof(ShowEmpty));

    private static HealthRecordListDisplayItem MapItem(HealthRecordListItemViewModel r, CultureInfo culture)
    {
        var start = r.VisitStart.ToLocalTime();
        var end = r.VisitEnd?.ToLocalTime();
        var when = end is null
            ? start.ToString("g", culture)
            : $"{start.ToString("g", culture)} – {end.Value.ToString("t", culture)}";
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
