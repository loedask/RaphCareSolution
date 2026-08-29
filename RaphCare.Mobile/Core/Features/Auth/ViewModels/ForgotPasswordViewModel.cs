using System.Windows.Input;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Auth.ViewModels;

/// <summary>Email/password forgot-password: request a code, then set a new password.</summary>
public sealed class ForgotPasswordViewModel : BaseViewModel
{
    private readonly IEmailAuthService _emailAuth;

    private string _email = string.Empty;
    private string _verificationCode = string.Empty;
    private string _newPassword = string.Empty;
    private string _confirmPassword = string.Empty;
    private string? _errorMessage;
    private string? _statusMessage;
    private bool _awaitingCode;

    public ForgotPasswordViewModel(IEmailAuthService emailAuth)
    {
        _emailAuth = emailAuth ?? throw new ArgumentNullException(nameof(emailAuth));
        Title = T("ForgotPasswordTitle");
        PageTitle = T("ForgotPasswordTitle");
        Subtitle = T("ForgotPasswordSubtitle");
        EmailLabel = T("AuthEmailLabel");
        EmailPlaceholder = T("AuthEmailPlaceholder");
        CodeLabel = T("ForgotPasswordCodeLabel");
        CodePlaceholder = T("AuthVerificationCodePlaceholder");
        NewPasswordLabel = T("ForgotPasswordNewLabel");
        ConfirmPasswordLabel = T("ForgotPasswordConfirmLabel");
        PasswordPlaceholder = T("AuthPasswordPlaceholder");
        SendCodeText = T("ForgotPasswordSendCode");
        ResetText = T("ForgotPasswordReset");
        BackCommand = new Command(async () => await GoBackAsync().ConfigureAwait(false));
        ContinueCommand = new Command(async () => await ContinueAsync().ConfigureAwait(false), () => !IsBusy);
    }

    public string PageTitle { get; }
    public string Subtitle { get; }
    public string EmailLabel { get; }
    public string EmailPlaceholder { get; }
    public string CodeLabel { get; }
    public string CodePlaceholder { get; }
    public string NewPasswordLabel { get; }
    public string ConfirmPasswordLabel { get; }
    public string PasswordPlaceholder { get; }
    public string SendCodeText { get; }
    public string ResetText { get; }

    public string ContinueButtonText => AwaitingCode ? ResetText : SendCodeText;

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value ?? string.Empty);
    }

    public string VerificationCode
    {
        get => _verificationCode;
        set => SetProperty(ref _verificationCode, value ?? string.Empty);
    }

    public string NewPassword
    {
        get => _newPassword;
        set => SetProperty(ref _newPassword, value ?? string.Empty);
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value ?? string.Empty);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool AwaitingCode
    {
        get => _awaitingCode;
        private set
        {
            if (_awaitingCode == value)
                return;
            _awaitingCode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ContinueButtonText));
        }
    }

    public ICommand BackCommand { get; }
    public ICommand ContinueCommand { get; }

    public void PrefillEmail(string? email)
    {
        if (!string.IsNullOrWhiteSpace(email))
            Email = email.Trim();
    }

    private async Task ContinueAsync()
    {
        if (IsBusy)
            return;

        ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = T("AuthEmailRequired");
            return;
        }

        IsBusy = true;
        BusyMessage = T("ForgotPasswordBusy");
        try
        {
            if (!AwaitingCode)
            {
                var send = await _emailAuth
                    .RequestPasswordResetAsync(Email.Trim(), CancellationToken.None)
                    .ConfigureAwait(false);
                if (!send.IsSuccess)
                {
                    ErrorMessage = send.ErrorMessage ?? T("ForgotPasswordSendFailed");
                    return;
                }

                AwaitingCode = true;
                StatusMessage = T("ForgotPasswordCodeSent");
                return;
            }

            if (string.IsNullOrWhiteSpace(VerificationCode))
            {
                ErrorMessage = T("RegisterEmailErrorVerificationCode");
                return;
            }

            if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword.Length < 8)
            {
                ErrorMessage = T("ForgotPasswordTooShort");
                return;
            }

            if (!string.Equals(NewPassword, ConfirmPassword, StringComparison.Ordinal))
            {
                ErrorMessage = T("ForgotPasswordMismatch");
                return;
            }

            var reset = await _emailAuth
                .ConfirmPasswordResetAsync(
                    Email.Trim(),
                    VerificationCode.Trim(),
                    NewPassword,
                    CancellationToken.None)
                .ConfigureAwait(false);

            if (!reset.IsSuccess || reset.Data is not { Success: true })
            {
                ErrorMessage = reset.ErrorMessage ?? T("ForgotPasswordResetFailed");
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.DisplayAlertAsync(
                        T("ForgotPasswordTitle"),
                        T("ForgotPasswordSuccess"),
                        T("CommonOk")).ConfigureAwait(false);
                    await SafeShellNavigator.GoToAsync(AppNavigator.SignIn).ConfigureAwait(false);
                })
                .ConfigureAwait(false);
        }
        catch (Exception)
        {
            ErrorMessage = AwaitingCode ? T("ForgotPasswordResetFailed") : T("ForgotPasswordSendFailed");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static async Task GoBackAsync()
    {
        if (Shell.Current.Navigation.NavigationStack.Count > 1)
            await SafeShellNavigator.GoToAsync("..").ConfigureAwait(false);
        else
            await SafeShellNavigator.GoToAsync(AppNavigator.SignIn).ConfigureAwait(false);
    }
}
