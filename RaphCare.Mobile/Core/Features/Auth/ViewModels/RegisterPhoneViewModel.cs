using System.Globalization;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Mobile.Core.Features.Auth.Models;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Auth.ViewModels;

/// <summary>Collect phone number and request OTP (<c>api/auth/otp/send</c>).</summary>
public class RegisterPhoneViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IOtpAuthService _otpAuth;

    private CountryDialOption _selectedCountry = CountryDialOption.DefaultList[3];
    private string _localNumber = string.Empty;
    private string? _errorMessage;
    private string _continueWith = string.Empty;

    public string Subtitle { get; }
    public string PhoneFieldLabel { get; }
    public string ContinueText { get; }

    public RegisterPhoneViewModel(IOtpAuthService otpAuth)
    {
        _otpAuth = otpAuth ?? throw new ArgumentNullException(nameof(otpAuth));
        Title = T("RegisterPhoneTitle");
        Subtitle = T("RegisterPhoneSubtitle");
        PhoneFieldLabel = T("RegisterPhoneFieldLabel");
        ContinueText = T("RegisterPhoneContinue");
        ContinueCommand = new Command(async () => await SendAndContinueAsync(), () => !IsBusy);
        BackCommand = new Command(async () => await GoBackAsync());
        Countries = CountryDialOption.DefaultList;
    }

    public IReadOnlyList<CountryDialOption> Countries { get; }

    public CountryDialOption SelectedCountry
    {
        get => _selectedCountry;
        set => SetProperty(ref _selectedCountry, value);
    }

    public string LocalNumber
    {
        get => _localNumber;
        set => SetProperty(ref _localNumber, value ?? string.Empty);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand ContinueCommand { get; }
    public ICommand BackCommand { get; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("ContinueWith", out var v) && v != null)
            _continueWith = Convert.ToString(v, CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private string BuildE164()
    {
        var digits = new string((LocalNumber ?? string.Empty).Where(char.IsDigit).ToArray());
        var code = (SelectedCountry?.DialCode ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(code))
            return digits.Length > 0 ? "+" + digits : string.Empty;
        if (!code.StartsWith('+'))
            code = "+" + code;
        return code + digits;
    }

    private async Task SendAndContinueAsync()
    {
        if (IsBusy) return;

        ErrorMessage = null;
        var phone = BuildE164();
        if (phone.Length < 10)
        {
            ErrorMessage = T("RegisterPhoneInvalid");
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _otpAuth.SendOtpAsync(phone, CancellationToken.None).ConfigureAwait(false);
            if (!result.IsSuccess)
            {
                ErrorMessage = result.ErrorMessage ?? T("RegisterPhoneSendFailed");
                return;
            }

            var qContinue = Uri.EscapeDataString(_continueWith);
            var qPhone = Uri.EscapeDataString(phone);
            await SafeShellNavigator.GoToAsync($"VerifyPhonePage?Phone={qPhone}&ContinueWith={qContinue}").ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task GoBackAsync()
    {
        if (Shell.Current.Navigation.NavigationStack.Count > 1)
            await SafeShellNavigator.GoToAsync("..");
        else if (string.Equals(_continueWith, "Voice", StringComparison.OrdinalIgnoreCase))
            await SafeShellNavigator.GoToAsync($"//{AppNavigator.Landing}/{AppNavigator.RegisterVoiceIntro}");
        else
            await SafeShellNavigator.GoToAsync($"//{AppNavigator.Landing}/{AppNavigator.RegisterOptions}");
    }
}
