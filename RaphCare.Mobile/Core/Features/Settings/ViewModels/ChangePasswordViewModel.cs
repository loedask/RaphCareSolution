using System.Windows.Input;
using Microsoft.Extensions.Options;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Mobile.Core.Common.Models;
using RaphCare.Mobile.Core.Common.Services.Auth;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Settings.ViewModels;

/// <summary>Change password (concept <c>ChangePassword.tsx</c>). Email users change in-app; Entra users open SSPR.</summary>
public sealed class ChangePasswordViewModel : BaseViewModel
{
    private readonly IAuthService _auth;
    private readonly IPatientAccountService _account;
    private readonly EntraAuthOptions _entra;
    private string _currentPassword = string.Empty;
    private string _newPassword = string.Empty;
    private string _confirmPassword = string.Empty;
    private bool _usesEmailPassword;
    private bool _usesEntraReset = true;

    public ChangePasswordViewModel(
        IAuthService auth,
        IPatientAccountService account,
        IOptions<EntraAuthOptions> entra)
    {
        _auth = auth ?? throw new ArgumentNullException(nameof(auth));
        _account = account ?? throw new ArgumentNullException(nameof(account));
        _entra = entra.Value;
        Title = T("ChangePasswordTitle");
        CurrentLabel = T("ChangePasswordCurrent");
        NewLabel = T("ChangePasswordNew");
        ConfirmLabel = T("ChangePasswordConfirm");
        UpdateLabel = T("ChangePasswordUpdate");
        EntraHint = T("ChangePasswordEntraHint");
        EntraResetLabel = T("ChangePasswordEntraReset");
        UpdateCommand = new Command(async () => await UpdateAsync(), () => UsesEmailPassword);
        OpenEntraResetCommand = new Command(async () => await OpenEntraResetAsync());
    }

    public string CurrentLabel { get; }
    public string NewLabel { get; }
    public string ConfirmLabel { get; }
    public string UpdateLabel { get; }
    public string EntraHint { get; }
    public string EntraResetLabel { get; }

    public bool UsesEmailPassword
    {
        get => _usesEmailPassword;
        private set
        {
            if (_usesEmailPassword == value)
                return;
            _usesEmailPassword = value;
            OnPropertyChanged(nameof(UsesEmailPassword));
            (UpdateCommand as Command)?.ChangeCanExecute();
        }
    }

    public bool UsesEntraReset
    {
        get => _usesEntraReset;
        private set => SetProperty(ref _usesEntraReset, value);
    }

    public string CurrentPassword
    {
        get => _currentPassword;
        set => SetProperty(ref _currentPassword, value);
    }

    public string NewPassword
    {
        get => _newPassword;
        set => SetProperty(ref _newPassword, value);
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    public ICommand UpdateCommand { get; }
    public ICommand OpenEntraResetCommand { get; }

    public async Task LoadAsync()
    {
        var kind = await _auth.GetAccountKindAsync(CancellationToken.None).ConfigureAwait(false);
        UsesEmailPassword = kind == AuthAccountKind.Email;
        UsesEntraReset = kind != AuthAccountKind.Email;
    }

    private async Task UpdateAsync()
    {
        if (!UsesEmailPassword)
            return;

        if (string.IsNullOrWhiteSpace(CurrentPassword) || string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            await AlertAsync(T("ChangePasswordMissingFields"));
            return;
        }

        if (!string.Equals(NewPassword, ConfirmPassword, StringComparison.Ordinal))
        {
            await AlertAsync(T("ChangePasswordMismatch"));
            return;
        }

        if (NewPassword.Length < 8)
        {
            await AlertAsync(T("ChangePasswordTooShort"));
            return;
        }

        IsBusy = true;
        try
        {
            var response = await _account
                .ChangePasswordAsync(CurrentPassword, NewPassword, CancellationToken.None)
                .ConfigureAwait(false);

            if (!response.IsSuccess)
            {
                await AlertAsync(response.ErrorMessage ?? T("ChangePasswordFailed"));
                return;
            }

            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
            await AlertAsync(T("ChangePasswordSuccess"));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OpenEntraResetAsync()
    {
        var url = string.IsNullOrWhiteSpace(_entra.B2CPasswordResetAuthority)
            ? _entra.SelfServicePasswordResetUrl
            : _entra.B2CPasswordResetAuthority;

        if (string.IsNullOrWhiteSpace(url))
        {
            await AlertAsync(T("ChangePasswordNoResetUrl"));
            return;
        }

        await Browser.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
    }

    private static Task AlertAsync(string message) =>
        MainThread.InvokeOnMainThreadAsync(async () =>
            await Shell.Current.DisplayAlertAsync(T("ChangePasswordTitle"), message, T("CommonOk")));
}
