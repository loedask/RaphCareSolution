using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using RaphCare.Mobile.Core.Features.Settings.Services;
using RaphCare.Mobile.Core.Shared.Navigation;
using RaphCare.Mobile.Core.Shared.Services.Auth;
using RaphCare.Mobile.Core.Shared.ViewModels;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Features.Settings.ViewModels;

/// <summary>Privacy &amp; data controls (concept <c>Privacy.tsx</c>). Toggles are device-local.</summary>
public sealed class PrivacySettingsViewModel : BaseViewModel
{
    private readonly ILocalPatientProfileStore _profile;
    private readonly IAuthService _auth;
    private bool _dataSharing;
    private bool _twoFactor;

    public PrivacySettingsViewModel(ILocalPatientProfileStore profile, IAuthService auth)
    {
        _profile = profile ?? throw new ArgumentNullException(nameof(profile));
        _auth = auth ?? throw new ArgumentNullException(nameof(auth));
        Title = AppResources.T("PrivacyTitle");
        ChangePasswordCommand = new Command(async () =>
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.DisplayAlertAsync(Title, AppResources.T("ProfileFeatureComingSoon"), AppResources.T("CommonOk"))));
        DownloadDataCommand = new Command(async () =>
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.DisplayAlertAsync(
                    AppResources.T("PrivacyDownloadTitle"),
                    AppResources.T("PrivacyDownloadMessage"),
                    AppResources.T("CommonOk"))));
        DeleteAccountCommand = new Command(async () => await ConfirmDeleteAccountAsync());

        SecuritySectionTitle = AppResources.T("PrivacySecuritySection");
        ChangePasswordTitle = AppResources.T("ProfileChangePassword");
        ChangePasswordSubtitle = AppResources.T("PrivacyChangePasswordSubtitle");
        TwoFactorTitle = AppResources.T("PrivacyTwoFactorTitle");
        TwoFactorSubtitle = AppResources.T("PrivacyTwoFactorSubtitle");
        DataSectionTitle = AppResources.T("PrivacyDataSection");
        DataSharingTitle = AppResources.T("PrivacyDataSharingTitle");
        DataSharingSubtitle = AppResources.T("PrivacyDataSharingSubtitle");
        DownloadTitle = AppResources.T("PrivacyDownloadRowTitle");
        DownloadSubtitle = AppResources.T("PrivacyDownloadRowSubtitle");
        DeleteTitle = AppResources.T("PrivacyDeleteRowTitle");
        DeleteSubtitle = AppResources.T("PrivacyDeleteRowSubtitle");
    }

    public string SecuritySectionTitle { get; }
    public string ChangePasswordTitle { get; }
    public string ChangePasswordSubtitle { get; }
    public string TwoFactorTitle { get; }
    public string TwoFactorSubtitle { get; }
    public string DataSectionTitle { get; }
    public string DataSharingTitle { get; }
    public string DataSharingSubtitle { get; }
    public string DownloadTitle { get; }
    public string DownloadSubtitle { get; }
    public string DeleteTitle { get; }
    public string DeleteSubtitle { get; }

    public bool DataSharing
    {
        get => _dataSharing;
        set
        {
            if (_dataSharing == value)
                return;
            _dataSharing = value;
            _profile.PrivacyDataSharing = value;
            OnPropertyChanged(nameof(DataSharing));
        }
    }

    public bool TwoFactor
    {
        get => _twoFactor;
        set
        {
            if (_twoFactor == value)
                return;
            _twoFactor = value;
            _profile.PrivacyTwoFactor = value;
            OnPropertyChanged(nameof(TwoFactor));
        }
    }

    public ICommand ChangePasswordCommand { get; }
    public ICommand DownloadDataCommand { get; }
    public ICommand DeleteAccountCommand { get; }

    public void LoadFromStore()
    {
        _dataSharing = _profile.PrivacyDataSharing;
        _twoFactor = _profile.PrivacyTwoFactor;
        OnPropertyChanged(nameof(DataSharing));
        OnPropertyChanged(nameof(TwoFactor));
    }

    private async Task ConfirmDeleteAccountAsync()
    {
        var ok = await MainThread.InvokeOnMainThreadAsync(async () =>
            await Shell.Current.DisplayAlertAsync(
                AppResources.T("PrivacyDeleteTitle"),
                AppResources.T("PrivacyDeleteMessage"),
                AppResources.T("PrivacyDeleteConfirm"),
                AppResources.T("CommonCancel")));
        if (!ok)
            return;

        await MainThread.InvokeOnMainThreadAsync(async () =>
            await Shell.Current.DisplayAlertAsync(
                AppResources.T("PrivacyDeleteRequestedTitle"),
                AppResources.T("PrivacyDeleteRequestedBody"),
                AppResources.T("CommonOk")));
        await _auth.SignOutAsync(CancellationToken.None);
        await SafeShellNavigator.GoToAsync("//" + AppNavigator.Landing);
    }
}
