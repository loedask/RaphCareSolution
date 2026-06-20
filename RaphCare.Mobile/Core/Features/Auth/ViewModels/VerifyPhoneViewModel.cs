using System.Windows.Input;
using Microsoft.Maui.Controls;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.Services.Auth;
using RaphCare.Mobile.Core.Common.ViewModels;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Features.Auth.ViewModels;

/// <summary>OTP entry; verify calls <c>api/auth/otp/verify</c> and stores the patient JWT.</summary>
public class VerifyPhoneViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IOtpAuthService _otpAuth;
    private readonly IAuthService _authService;

    private string _phoneE164 = string.Empty;
    private string _continueWith = string.Empty;
    private string _code = string.Empty;
    private string? _errorMessage;

    public VerifyPhoneViewModel(IOtpAuthService otpAuth, IAuthService authService)
    {
        _otpAuth = otpAuth ?? throw new ArgumentNullException(nameof(otpAuth));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        Title = AppResources.T("VerifyPhoneTitle");
        Subtitle = AppResources.T("VerifyPhoneSubtitle");
        CodeLabel = AppResources.T("VerifyPhoneCodeLabel");
        VerifyText = AppResources.T("VerifyPhoneVerify");
        ResendText = AppResources.T("VerifyPhoneResend");

        VerifyCommand = new Command(async () => await VerifyAsync(), () => !IsBusy);
        ResendCommand = new Command(async () => await ResendAsync(), () => !IsBusy);
        BackCommand = new Command(async () => await GoBackAsync());
    }

    public string Subtitle { get; }
    public string CodeLabel { get; }
    public string VerifyText { get; }
    public string ResendText { get; }

    public string PhoneE164
    {
        get => _phoneE164;
        private set => SetProperty(ref _phoneE164, value);
    }

    public string Code
    {
        get => _code;
        set => SetProperty(ref _code, value ?? string.Empty);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand VerifyCommand { get; }
    public ICommand ResendCommand { get; }
    public ICommand BackCommand { get; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Phone", out var p) && p != null)
            PhoneE164 = p.ToString() ?? string.Empty;
        if (query.TryGetValue("ContinueWith", out var c) && c != null)
            _continueWith = c.ToString() ?? string.Empty;
    }

    private async Task VerifyAsync()
    {
        if (IsBusy) return;

        ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(PhoneE164))
        {
            ErrorMessage = AppResources.T("RegisterPhoneInvalid");
            return;
        }

        if (string.IsNullOrWhiteSpace(Code) || Code.Trim().Length < 4)
        {
            ErrorMessage = AppResources.T("VerifyPhoneCodeInvalid");
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _otpAuth.VerifyOtpAsync(PhoneE164, Code.Trim(), CancellationToken.None).ConfigureAwait(false);
            if (!result.IsSuccess || result.Data is null)
            {
                ErrorMessage = result.ErrorMessage ?? AppResources.T("VerifyPhoneFailed");
                return;
            }

            if (!result.Data.Success || string.IsNullOrWhiteSpace(result.Data.Token))
            {
                ErrorMessage = AppResources.T("VerifyPhoneFailed");
                return;
            }

            // Matches RaphCare.Infrastructure TokenService patient token lifetime (12h).
            var expires = DateTimeOffset.UtcNow.AddHours(12);
            await _authService.StoreApiSessionAsync(result.Data.Token, expires, CancellationToken.None).ConfigureAwait(false);

            if (string.Equals(_continueWith, "Voice", StringComparison.OrdinalIgnoreCase))
            {
                var q = Uri.EscapeDataString(PhoneE164);
                await SafeShellNavigator.GoToAsync($"VoiceSubmitPage?Phone={q}").ConfigureAwait(false);
            }
            else
            {
                await SafeShellNavigator.GoToAsync($"//{AppNavigator.AccountCreated}").ConfigureAwait(false);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ResendAsync()
    {
        if (IsBusy || string.IsNullOrWhiteSpace(PhoneE164)) return;

        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var result = await _otpAuth.SendOtpAsync(PhoneE164, CancellationToken.None).ConfigureAwait(false);
            if (!result.IsSuccess)
                ErrorMessage = result.ErrorMessage ?? AppResources.T("RegisterPhoneSendFailed");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task GoBackAsync()
    {
        await SafeShellNavigator.GoToAsync("..").ConfigureAwait(false);
    }
}
