using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Telehealth;
using RaphCare.Mobile.Core.Features.CareTelehealth.Models;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Features.CareTelehealth.ViewModels;

public sealed class CareTelehealthViewModel : BaseViewModel
{
    private readonly IPatientTelehealthService _telehealth;
    private string? _errorMessage;

    public CareTelehealthViewModel(IPatientTelehealthService telehealth)
    {
        _telehealth = telehealth ?? throw new ArgumentNullException(nameof(telehealth));
        Title = AppResources.T("CareTelehealthTitle");
        RefreshButtonText = AppResources.T("CareTelehealthRefresh");
        EmptyStateText = AppResources.T("CareTelehealthEmpty");
        HintText = AppResources.T("CareTelehealthHint");

        RefreshCommand = new Command(async () => await LoadAsync());
        OpenJoinCommand = new Command<Guid>(async id =>
            await SafeShellNavigator.GoToAsync($"{AppNavigator.TelehealthJoin}?sessionId={id}"));

        Items.CollectionChanged += (_, _) => OnPropertyChanged(nameof(ShowEmpty));
    }

    public ObservableCollection<TeleSessionListDisplayItem> Items { get; } = new();

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool ShowEmpty => !IsBusy && Items.Count == 0 && string.IsNullOrEmpty(ErrorMessage);

    public string RefreshButtonText { get; }
    public string EmptyStateText { get; }
    public string HintText { get; }

    public ICommand RefreshCommand { get; }
    public ICommand OpenJoinCommand { get; }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        ErrorMessage = null;
        IsBusy = true;
        OnPropertyChanged(nameof(ShowEmpty));
        try
        {
            var response = await _telehealth.GetMySessionsAsync(1, 50, CancellationToken.None).ConfigureAwait(false);
            if (!response.IsSuccess || response.Data is null)
            {
                ErrorMessage = response.ErrorMessage ?? AppResources.T("CareTelehealthLoadFailed");
                Items.Clear();
                OnPropertyChanged(nameof(ShowEmpty));
                return;
            }

            Items.Clear();
            var culture = CultureInfo.CurrentCulture;
            foreach (var s in response.Data.Items)
                Items.Add(MapItem(s, culture));
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(ShowEmpty));
        }
    }

    private static TeleSessionListDisplayItem MapItem(PatientTeleSessionListItemViewModel s, CultureInfo culture)
    {
        var when = s.ScheduledStart.ToLocalTime().ToString("g", culture);
        return new TeleSessionListDisplayItem
        {
            Id = s.Id,
            PrimaryLine = $"{when} · {s.Status}",
            SecondaryLine = string.IsNullOrWhiteSpace(s.Platform) ? AppResources.T("CareTelehealthPlatformUnknown") : s.Platform
        };
    }
}
