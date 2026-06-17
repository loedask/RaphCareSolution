using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Billing;
using RaphCare.Mobile.Core.Shared.Navigation;
using RaphCare.Mobile.Core.Shared.ViewModels;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Features.Billing.ViewModels;

public sealed class BillingViewModel : BaseViewModel
{
    private readonly IPatientBillingService _billing;
    private string? _errorMessage;
    private PatientCarePlanViewModel? _carePlan;
    private PatientBillingPlanOptionViewModel? _selectedUpgradePlan;
    private string _currentPlanSummary = string.Empty;

    public BillingViewModel(IPatientBillingService billing)
    {
        _billing = billing ?? throw new ArgumentNullException(nameof(billing));
        Title = AppResources.T("BillingTitle");
        RefreshButtonText = AppResources.T("BillingRefresh");
        AddPaymentButtonText = AppResources.T("BillingAddPayment");
        UpgradeSectionTitle = AppResources.T("BillingSectionUpgrade");
        UpgradePickerTitle = AppResources.T("BillingPickerUpgradePlan");
        UpgradeButtonText = AppResources.T("BillingUpgradeButton");
        PaymentMethodsSectionTitle = AppResources.T("BillingSectionPaymentMethods");
        HistorySectionTitle = AppResources.T("BillingSectionHistory");
        CurrentPlanLabel = AppResources.T("BillingCurrentPlanLabel");
        SetDefaultButtonText = AppResources.T("BillingSetDefault");
        RemoveButtonText = AppResources.T("BillingRemove");
        EmptyPaymentsText = AppResources.T("BillingEmptyPayments");
        EmptyInvoicesText = AppResources.T("BillingEmptyInvoices");

        RefreshCommand = new Command(async () => await LoadAsync());
        AddPaymentCommand = new Command(async () => await SafeShellNavigator.GoToAsync(AppNavigator.AddBillingPaymentMethod));
        UpgradeCommand = new Command(async () => await UpgradeAsync(), () => SelectedUpgradePlan != null);
        SetDefaultCommand = new Command<Guid>(async id => await SetDefaultAsync(id));
        RemoveMethodCommand = new Command<Guid>(async id => await RemoveAsync(id));

        PlanOptions.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasUpgradeChoices));
            NotifyEmptyStates();
        };
        PaymentMethods.CollectionChanged += (_, _) => NotifyEmptyStates();
        Invoices.CollectionChanged += (_, _) => NotifyEmptyStates();
    }

    private void NotifyEmptyStates()
    {
        OnPropertyChanged(nameof(ShowEmptyPayments));
        OnPropertyChanged(nameof(ShowEmptyInvoices));
    }

    private void ClearBillingLists()
    {
        CurrentPlanSummary = string.Empty;
        PlanOptions.Clear();
        PaymentMethods.Clear();
        Invoices.Clear();
        OnPropertyChanged(nameof(HasUpgradeChoices));
        SelectedUpgradePlan = null;
    }

    public string RefreshButtonText { get; }
    public string AddPaymentButtonText { get; }
    public string UpgradeSectionTitle { get; }
    public string UpgradePickerTitle { get; }
    public string UpgradeButtonText { get; }
    public string PaymentMethodsSectionTitle { get; }
    public string HistorySectionTitle { get; }
    public string CurrentPlanLabel { get; }
    public string SetDefaultButtonText { get; }
    public string RemoveButtonText { get; }
    public string EmptyPaymentsText { get; }
    public string EmptyInvoicesText { get; }

    public ObservableCollection<PatientBillingPlanOptionViewModel> PlanOptions { get; } = new();
    public ObservableCollection<PatientPaymentMethodViewModel> PaymentMethods { get; } = new();
    public ObservableCollection<PatientInvoiceHistoryItemViewModel> Invoices { get; } = new();

    public PatientBillingPlanOptionViewModel? SelectedUpgradePlan
    {
        get => _selectedUpgradePlan;
        set
        {
            if (EqualityComparer<PatientBillingPlanOptionViewModel?>.Default.Equals(_selectedUpgradePlan, value))
                return;
            SetProperty(ref _selectedUpgradePlan, value);
            ((Command)UpgradeCommand).ChangeCanExecute();
        }
    }

    public string CurrentPlanSummary
    {
        get => _currentPlanSummary;
        private set => SetProperty(ref _currentPlanSummary, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (EqualityComparer<string?>.Default.Equals(_errorMessage, value))
                return;
            SetProperty(ref _errorMessage, value);
            NotifyEmptyStates();
        }
    }

    public bool HasUpgradeChoices => PlanOptions.Count > 0;

    public bool ShowEmptyPayments => !IsBusy && PaymentMethods.Count == 0 && string.IsNullOrEmpty(ErrorMessage);

    public bool ShowEmptyInvoices => !IsBusy && Invoices.Count == 0 && string.IsNullOrEmpty(ErrorMessage);

    public ICommand RefreshCommand { get; }
    public ICommand AddPaymentCommand { get; }
    public ICommand UpgradeCommand { get; }
    public ICommand SetDefaultCommand { get; }
    public ICommand RemoveMethodCommand { get; }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        ErrorMessage = null;
        IsBusy = true;
        NotifyEmptyStates();
        try
        {
            var plans = await _billing.GetPlanOptionsAsync(CancellationToken.None).ConfigureAwait(false);
            var care = await _billing.GetMyCarePlanAsync(CancellationToken.None).ConfigureAwait(false);
            var methods = await _billing.GetMyPaymentMethodsAsync(CancellationToken.None).ConfigureAwait(false);
            var inv = await _billing.GetMyInvoicesAsync(1, 30, CancellationToken.None).ConfigureAwait(false);

            if (!plans.IsSuccess || plans.Data is null)
            {
                ErrorMessage = plans.ErrorMessage ?? AppResources.T("BillingLoadFailed");
                ClearBillingLists();
                return;
            }

            if (!care.IsSuccess || care.Data is null)
            {
                ErrorMessage = care.ErrorMessage ?? AppResources.T("BillingLoadFailed");
                ClearBillingLists();
                return;
            }

            _carePlan = care.Data;
            var culture = CultureInfo.CurrentCulture;
            CurrentPlanSummary = string.Format(culture, AppResources.T("BillingCurrentPlanFormat"), _carePlan.PlanDisplayName, _carePlan.Status);
            if (_carePlan.RenewsOn is { } r)
                CurrentPlanSummary += " · " + string.Format(culture, AppResources.T("BillingRenewsFormat"), r.ToLocalTime().ToString("d", culture));

            PlanOptions.Clear();
            foreach (var p in plans.Data.Where(x => x.Tier != _carePlan.Tier))
                PlanOptions.Add(p);
            OnPropertyChanged(nameof(HasUpgradeChoices));

            PaymentMethods.Clear();
            if (methods.IsSuccess && methods.Data is not null)
            {
                foreach (var m in methods.Data)
                    PaymentMethods.Add(m);
            }

            Invoices.Clear();
            if (inv.IsSuccess && inv.Data is not null)
            {
                foreach (var i in inv.Data.Items)
                    Invoices.Add(i);
            }
        }
        finally
        {
            IsBusy = false;
            NotifyEmptyStates();
        }
    }

    private async Task UpgradeAsync()
    {
        if (SelectedUpgradePlan is null) return;
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var res = await _billing.UpgradePlanAsync(SelectedUpgradePlan.PlanCode, CancellationToken.None).ConfigureAwait(false);
            if (!res.IsSuccess)
            {
                ErrorMessage = res.ErrorMessage ?? AppResources.T("BillingUpgradeFailed");
                return;
            }

            SelectedUpgradePlan = null;
            await LoadAsync().ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
            NotifyEmptyStates();
        }
    }

    private async Task SetDefaultAsync(Guid id)
    {
        IsBusy = true;
        try
        {
            var res = await _billing.SetDefaultPaymentMethodAsync(id, CancellationToken.None).ConfigureAwait(false);
            if (!res.IsSuccess)
                ErrorMessage = res.ErrorMessage ?? AppResources.T("BillingLoadFailed");
            else
                await LoadAsync().ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
            NotifyEmptyStates();
        }
    }

    private async Task RemoveAsync(Guid id)
    {
        IsBusy = true;
        try
        {
            var res = await _billing.RemovePaymentMethodAsync(id, CancellationToken.None).ConfigureAwait(false);
            if (!res.IsSuccess)
                ErrorMessage = res.ErrorMessage ?? AppResources.T("BillingLoadFailed");
            else
                await LoadAsync().ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
            NotifyEmptyStates();
        }
    }
}
