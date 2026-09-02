using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Billing;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;

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
        Title = T("BillingTitle");
        RefreshButtonText = T("BillingRefresh");
        AddPaymentButtonText = T("BillingAddPayment");
        UpgradeSectionTitle = T("BillingSectionUpgrade");
        UpgradePickerTitle = T("BillingPickerUpgradePlan");
        UpgradeButtonText = T("BillingUpgradeButton");
        PaymentMethodsSectionTitle = T("BillingSectionPaymentMethods");
        HistorySectionTitle = T("BillingSectionHistory");
        CurrentPlanLabel = T("BillingCurrentPlanLabel");
        SetDefaultButtonText = T("BillingSetDefault");
        RemoveButtonText = T("BillingRemove");
        EmptyPaymentsText = T("BillingEmptyPayments");
        EmptyInvoicesText = T("BillingEmptyInvoices");

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
        SelectedUpgradePlan = null;
        PlanOptions.Clear();
        PaymentMethods.Clear();
        Invoices.Clear();
        OnPropertyChanged(nameof(HasUpgradeChoices));
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
            RaiseCanExecuteChanged(UpgradeCommand);
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
                var failMessage = plans.ErrorMessage ?? T("BillingLoadFailed");
                await RunOnMainThreadAsync(() =>
                {
                    ErrorMessage = failMessage;
                    ClearBillingLists();
                });
                return;
            }

            if (!care.IsSuccess || care.Data is null)
            {
                var failMessage = care.ErrorMessage ?? T("BillingLoadFailed");
                await RunOnMainThreadAsync(() =>
                {
                    ErrorMessage = failMessage;
                    ClearBillingLists();
                });
                return;
            }

            var carePlan = care.Data;
            var summary = Format(T("BillingCurrentPlanFormat"), carePlan.PlanDisplayName, carePlan.Status);
            if (carePlan.RenewsOn is { } r)
                summary += " · " + Format(T("BillingRenewsFormat"), r.ToLocalTime().ToString("d", CultureInfo.CurrentCulture));

            var planOptions = plans.Data.Where(x => x.Tier != carePlan.Tier).ToList();
            var paymentMethods = methods.IsSuccess && methods.Data is not null
                ? methods.Data.ToList()
                : [];
            var invoices = inv.IsSuccess && inv.Data is not null
                ? inv.Data.Items.ToList()
                : [];

            await RunOnMainThreadAsync(() =>
            {
                _carePlan = carePlan;
                CurrentPlanSummary = summary;

                SelectedUpgradePlan = null;
                PlanOptions.Clear();
                foreach (var p in planOptions)
                    PlanOptions.Add(p);
                OnPropertyChanged(nameof(HasUpgradeChoices));

                PaymentMethods.Clear();
                foreach (var m in paymentMethods)
                    PaymentMethods.Add(m);

                Invoices.Clear();
                foreach (var i in invoices)
                    Invoices.Add(i);
            });
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
                ErrorMessage = res.ErrorMessage ?? T("BillingUpgradeFailed");
                return;
            }

            await RunOnMainThreadAsync(() => SelectedUpgradePlan = null).ConfigureAwait(false);
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
                ErrorMessage = res.ErrorMessage ?? T("BillingLoadFailed");
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
                ErrorMessage = res.ErrorMessage ?? T("BillingLoadFailed");
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
