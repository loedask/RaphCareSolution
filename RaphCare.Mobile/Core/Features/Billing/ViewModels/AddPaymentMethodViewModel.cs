using System.Windows.Input;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Billing.ViewModels;

public sealed class AddPaymentMethodViewModel : BaseViewModel
{
    private readonly IPatientBillingService _billing;
    private string? _errorMessage;
    private string _methodType = "Card";
    private string _providerName = string.Empty;
    private string _maskedDetails = string.Empty;
    private bool _setAsDefault = true;

    public AddPaymentMethodViewModel(IPatientBillingService billing)
    {
        _billing = billing ?? throw new ArgumentNullException(nameof(billing));
        Title = T("BillingAddPaymentTitle");

    MethodTypeLabel = T("BillingFieldMethodType");
    ProviderLabel = T("BillingFieldProvider");
    MaskedLabel = T("BillingFieldMasked");
    DefaultLabel = T("BillingFieldDefault");
    SubmitLabel = T("BillingSubmitPayment");
    CancelLabel = T("BillingCancel");
        SubmitCommand = new Command(async () => await SubmitAsync(), () => !IsBusy && !string.IsNullOrWhiteSpace(ProviderName) && !string.IsNullOrWhiteSpace(MaskedDetails));
        CancelCommand = new Command(async () => await SafeShellNavigator.GoToAsync(".."));
    }

    public string MethodType
    {
        get => _methodType;
        set => SetProperty(ref _methodType, value);
    }

    public string ProviderName
    {
        get => _providerName;
        set
        {
            if (EqualityComparer<string>.Default.Equals(_providerName, value))
                return;
            SetProperty(ref _providerName, value);
            ((Command)SubmitCommand).ChangeCanExecute();
        }
    }

    public string MaskedDetails
    {
        get => _maskedDetails;
        set
        {
            if (EqualityComparer<string>.Default.Equals(_maskedDetails, value))
                return;
            SetProperty(ref _maskedDetails, value);
            ((Command)SubmitCommand).ChangeCanExecute();
        }
    }

    public bool SetAsDefault
    {
        get => _setAsDefault;
        set => SetProperty(ref _setAsDefault, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public string MethodTypeLabel { get; }
    public string ProviderLabel { get; }
    public string MaskedLabel { get; }
    public string DefaultLabel { get; }
    public string SubmitLabel { get; }
    public string CancelLabel { get; }

    public ICommand SubmitCommand { get; }
    public ICommand CancelCommand { get; }

    private async Task SubmitAsync()
    {
        if (IsBusy) return;
        ErrorMessage = null;
        IsBusy = true;
        ((Command)SubmitCommand).ChangeCanExecute();
        try
        {
            var res = await _billing.AddPaymentMethodAsync(
                MethodType.Trim(),
                ProviderName.Trim(),
                MaskedDetails.Trim(),
                SetAsDefault,
                CancellationToken.None).ConfigureAwait(false);
            if (!res.IsSuccess)
            {
                ErrorMessage = res.ErrorMessage ?? T("BillingAddPaymentFailed");
                return;
            }

            await SafeShellNavigator.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
            ((Command)SubmitCommand).ChangeCanExecute();
        }
    }
}
