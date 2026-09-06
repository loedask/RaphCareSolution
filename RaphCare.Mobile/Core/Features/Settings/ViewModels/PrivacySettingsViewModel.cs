using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using RaphCare.Mobile.Core.Features.Settings.Services;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Settings.ViewModels;

/// <summary>Privacy and data controls. Password change is live; other toggles are phone-local preferences.</summary>
public sealed class PrivacySettingsViewModel : BaseViewModel
{
    private readonly ILocalPatientProfileStore _profile;
    private bool _dataSharing;
    private bool _twoFactor;

    public PrivacySettingsViewModel(ILocalPatientProfileStore profile)
    {
        _profile = profile ?? throw new ArgumentNullException(nameof(profile));
        Title = T("PrivacyTitle");
        ChangePasswordCommand = new Command(async () => await SafeShellNavigator.GoToAsync(AppNavigator.ChangePassword));
        DownloadDataCommand = new Command(async () =>
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.DisplayAlertAsync(
                    T("PrivacyDownloadTitle"),
                    T("PrivacyDownloadMessage"),
                    T("CommonOk"))));
        DeleteAccountCommand = new Command(async () => await ConfirmDeleteAccountAsync());
        ContactSupportCommand = new Command(async () => await SafeShellNavigator.GoToAsync(AppNavigator.HelpSupport));

        SecuritySectionTitle = T("PrivacySecuritySection");
        ChangePasswordTitle = T("ProfileChangePassword");
        ChangePasswordSubtitle = T("PrivacyChangePasswordSubtitle");
        TwoFactorTitle = T("PrivacyTwoFactorTitle");
        TwoFactorSubtitle = T("PrivacyTwoFactorSubtitle");
        DataSectionTitle = T("PrivacyDataSection");
        DataSharingTitle = T("PrivacyDataSharingTitle");
        DataSharingSubtitle = T("PrivacyDataSharingSubtitle");
        DownloadTitle = T("PrivacyDownloadRowTitle");
        DownloadSubtitle = T("PrivacyDownloadRowSubtitle");
        DeleteTitle = T("PrivacyDeleteRowTitle");
        DeleteSubtitle = T("PrivacyDeleteRowSubtitle");
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
    public ICommand ContactSupportCommand { get; }

    public void LoadFromStore()
    {
        _dataSharing = _profile.PrivacyDataSharing;
        _twoFactor = _profile.PrivacyTwoFactor;
        OnPropertyChanged(nameof(DataSharing));
        OnPropertyChanged(nameof(TwoFactor));
    }

    private static async Task ConfirmDeleteAccountAsync()
    {
        var goToSupport = await MainThread.InvokeOnMainThreadAsync(async () =>
            await Shell.Current.DisplayAlertAsync(
                T("PrivacyDeleteTitle"),
                T("PrivacyDeleteMessage"),
                T("PrivacyDeleteContactSupport"),
                T("CommonCancel")));
        if (!goToSupport)
            return;

        await SafeShellNavigator.GoToAsync(AppNavigator.HelpSupport);
    }
}
