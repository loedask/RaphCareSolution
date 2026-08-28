using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Insurance;
using RaphCare.Mobile.Core.Features.Insurance.Models;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Insurance.ViewModels;

public sealed class InsuranceViewModel : BaseViewModel
{
    private readonly IPatientInsuranceService _insurance;
    private string? _errorMessage;

    public InsuranceViewModel(IPatientInsuranceService insurance)
    {
        _insurance = insurance ?? throw new ArgumentNullException(nameof(insurance));
        Title = T("InsuranceListTitle");
        RefreshButtonText = T("InsuranceRefresh");
        AddButtonText = T("InsuranceAdd");
        EmptyStateText = T("InsuranceEmpty");

        RefreshCommand = new Command(async () => await LoadAsync());
        AddCommand = new Command(async () => await SafeShellNavigator.GoToAsync(AppNavigator.AddInsuranceProfile));
        OpenDetailCommand = new Command<Guid>(async id => await SafeShellNavigator.GoToAsync($"{AppNavigator.InsuranceProfileDetail}?profileId={id}"));

        Items.CollectionChanged += (_, _) => NotifyEmptyChanged();
    }

    public ObservableCollection<InsuranceProfileListDisplayItem> Items { get; } = new();

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool ShowEmpty => !IsBusy && Items.Count == 0 && string.IsNullOrEmpty(ErrorMessage);

    public string RefreshButtonText { get; }
    public string AddButtonText { get; }
    public string EmptyStateText { get; }

    public ICommand RefreshCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand OpenDetailCommand { get; }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        ErrorMessage = null;
        IsBusy = true;
        NotifyEmptyChanged();
        try
        {
            var response = await _insurance.GetMyProfilesAsync(1, 50, CancellationToken.None).ConfigureAwait(false);
            if (!response.IsSuccess || response.Data is null)
            {
                ErrorMessage = response.ErrorMessage ?? T("InsuranceLoadFailed");
                Items.Clear();
                NotifyEmptyChanged();
                return;
            }

            Items.Clear();
            var culture = CultureInfo.CurrentCulture;
            foreach (var p in response.Data.Items)
                Items.Add(MapItem(p, culture));
        }
        finally
        {
            IsBusy = false;
            NotifyEmptyChanged();
        }
    }

    private void NotifyEmptyChanged() => OnPropertyChanged(nameof(ShowEmpty));

    private static InsuranceProfileListDisplayItem MapItem(PatientInsuranceProfileViewModel p, CultureInfo culture)
    {
        var plan = string.IsNullOrWhiteSpace(p.PlanName) ? p.PlanCode : p.PlanName;
        var active = p.IsActive ? T("InsuranceStatusActive") : T("InsuranceStatusInactive");
        var start = p.StartDate.ToLocalTime().ToString("d", culture);
        return new InsuranceProfileListDisplayItem
        {
            Id = p.Id,
            PrimaryLine = string.IsNullOrWhiteSpace(plan) ? p.MembershipNumber : $"{plan}",
            SecondaryLine = $"{p.MembershipNumber} · {active} · {start}"
        };
    }
}
