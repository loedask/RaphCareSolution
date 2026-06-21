using System.Windows.Input;
using Microsoft.Extensions.Options;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using RaphCare.Mobile.Core.Common.Services.Auth;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Settings.ViewModels;

/// <summary>Change password (concept <c>ChangePassword.tsx</c>). Entra users open SSPR; email users see guidance.</summary>
public sealed class ChangePasswordViewModel : BaseViewModel
{
    private readonly EntraAuthOptions _entra;
    private string _currentPassword = string.Empty;
    private string _newPassword = string.Empty;
    private string _confirmPassword = string.Empty;
    private bool _showCurrent;
    private bool _showNew;
    private bool _showConfirm;

    public ChangePasswordViewModel(IOptions<EntraAuthOptions> entra)
    {
        _entra = entra.Value;
        Title = T("ChangePasswordTitle");
        CurrentLabel = T("ChangePasswordCurrent");
        NewLabel = T("ChangePasswordNew");
        ConfirmLabel = T("ChangePasswordConfirm");
        UpdateLabel = T("ChangePasswordUpdate");
        EntraHint = T("ChangePasswordEntraHint");
        UpdateCommand = new Command(async () => await UpdateAsync());
        OpenEntraResetCommand = new Command(async () => await OpenEntraResetAsync());
    }

    public string CurrentLabel { get; }
    public string NewLabel { get; }
    public string ConfirmLabel { get; }
    public string UpdateLabel { get; }
    public string EntraHint { get; }

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

    public bool ShowCurrent
    {
        get => _showCurrent;
        set => SetProperty(ref _showCurrent, value);
    }

    public bool ShowNew
    {
        get => _showNew;
        set => SetProperty(ref _showNew, value);
    }

    public bool ShowConfirm
    {
        get => _showConfirm;
        set => SetProperty(ref _showConfirm, value);
    }

    public ICommand UpdateCommand { get; }
    public ICommand OpenEntraResetCommand { get; }

    private async Task UpdateAsync()
    {
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

        await AlertAsync(T("ChangePasswordEmailGuidance"));
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
