using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Mobile.Core.Features.Settings.Models;
using RaphCare.Mobile.Core.Features.Settings.Services;
using RaphCare.Mobile.Core.Shared.Navigation;
using RaphCare.Mobile.Core.Shared.Services.Auth;
using RaphCare.Mobile.Core.Shared.ViewModels;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Features.Settings.ViewModels;

/// <summary>Profile hub (concept <c>Profile.tsx</c>): avatar, grouped links, sign out.</summary>
public sealed class ProfileHubViewModel : BaseViewModel
{
    private readonly IAuthService _auth;
    private readonly IPatientProfileService _profileApi;
    private readonly ILocalPatientProfileStore _localProfile;
    private string _displayName = string.Empty;
    private string _emailLine = string.Empty;

    public ProfileHubViewModel(IAuthService auth, IPatientProfileService profileApi, ILocalPatientProfileStore localProfile)
    {
        _auth = auth ?? throw new ArgumentNullException(nameof(auth));
        _profileApi = profileApi ?? throw new ArgumentNullException(nameof(profileApi));
        _localProfile = localProfile ?? throw new ArgumentNullException(nameof(localProfile));

        Title = AppResources.T("ProfileHubTitle");
        EditProfileCommand = new Command(async () => await SafeShellNavigator.GoToAsync(AppNavigator.EditProfile));
        SignOutCommand = new Command(async () => await SignOutAsync());
        OpenPhotoCommand = new Command(async () =>
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.DisplayAlertAsync(Title, AppResources.T("ProfilePhotoComingSoon"), AppResources.T("CommonOk"))));
        Sections = new ObservableCollection<ProfileSectionModel>();
        AppVersionLabel = $"{AppResources.T("ProfileAppName")} v{AppInfo.Current.VersionString}";
        InsuranceBadgeText = AppResources.T("ProfileInsuranceBadge");
        EditProfileButtonText = AppResources.T("ProfileEditProfile");
        SignOutButtonText = AppResources.T("ProfileSignOut");
    }

    public string EditProfileButtonText { get; }
    public string SignOutButtonText { get; }

    public string DisplayName
    {
        get => _displayName;
        private set => SetProperty(ref _displayName, value);
    }

    public string EmailLine
    {
        get => _emailLine;
        private set => SetProperty(ref _emailLine, value);
    }

    public string InsuranceBadgeText { get; }
    public string AppVersionLabel { get; }

    public ObservableCollection<ProfileSectionModel> Sections { get; }

    public ICommand EditProfileCommand { get; }
    public ICommand SignOutCommand { get; }
    public ICommand OpenPhotoCommand { get; }

    public async Task LoadAsync()
    {
        var response = await _profileApi.GetMyProfileAsync(CancellationToken.None).ConfigureAwait(false);
        if (response.IsSuccess && response.Data is { } data)
        {
            _localProfile.FirstName = data.FirstName;
            _localProfile.LastName = data.LastName;
            _localProfile.Email = data.Email ?? string.Empty;
            _localProfile.Phone = data.PhoneNumber ?? string.Empty;
            _localProfile.DateOfBirth = data.DateOfBirth;
            _localProfile.Gender = data.Gender;
            await ApplyProfileDisplayAsync(data.FirstName, data.LastName, data.Email).ConfigureAwait(false);
            return;
        }

        var token = await _auth.GetAccessTokenAsync(CancellationToken.None).ConfigureAwait(false);
        var (claimName, claimEmail) = JwtClaimsReader.ReadDisplayClaims(token);

        var first = _localProfile.FirstName.Trim();
        var last = _localProfile.LastName.Trim();
        await ApplyProfileDisplayAsync(first, last, _localProfile.Email.Trim(), claimName, claimEmail).ConfigureAwait(false);
    }

    private Task ApplyProfileDisplayAsync(
        string firstName,
        string lastName,
        string? profileEmail,
        string? claimNameFallback = null,
        string? claimEmailFallback = null) =>
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            SetDisplayFromNames(firstName, lastName, profileEmail, claimNameFallback, claimEmailFallback);
            BuildSections();
        });

    private void SetDisplayFromNames(
        string firstName,
        string lastName,
        string? profileEmail,
        string? claimNameFallback = null,
        string? claimEmailFallback = null)
    {
        var combined = string.Join(" ", new[] { firstName.Trim(), lastName.Trim() }.Where(s => s.Length > 0));
        DisplayName = !string.IsNullOrEmpty(combined)
            ? combined
            : (!string.IsNullOrEmpty(claimNameFallback) ? claimNameFallback : AppResources.T("ProfileDefaultName"));

        var email = profileEmail?.Trim() ?? string.Empty;
        EmailLine = !string.IsNullOrEmpty(email)
            ? email
            : (!string.IsNullOrEmpty(claimEmailFallback) ? claimEmailFallback : AppResources.T("ProfileNoEmail"));
    }

    private void BuildSections()
    {
        Sections.Clear();

        Sections.Add(new ProfileSectionModel
        {
            Title = AppResources.T("ProfileSectionAccount"),
            Items = new ObservableCollection<ProfileMenuRowModel>(
            [
                new ProfileMenuRowModel
                {
                    Title = AppResources.T("ProfilePersonalInformation"),
                    Subtitle = AppResources.T("ProfilePersonalInformationHint"),
                    IconGlyph = "👤",
                    TapCommand = new Command(async () => await SafeShellNavigator.GoToAsync(AppNavigator.EditProfile)),
                },
                new ProfileMenuRowModel
                {
                    Title = AppResources.T("ProfileChangePassword"),
                    Subtitle = AppResources.T("ProfileChangePasswordHint"),
                    IconGlyph = "🔒",
                    TapCommand = new Command(async () => await ComingSoonAsync()),
                },
                new ProfileMenuRowModel
                {
                    Title = AppResources.T("ProfileLanguage"),
                    Subtitle = CurrentLanguageSubtitle(),
                    IconGlyph = "🌐",
                    ShowSeparator = false,
                    TapCommand = new Command(async () => await LanguageHintAsync()),
                },
            ]),
        });

        Sections.Add(new ProfileSectionModel
        {
            Title = AppResources.T("ProfileSectionHealth"),
            Items = new ObservableCollection<ProfileMenuRowModel>(
            [
                new ProfileMenuRowModel
                {
                    Title = AppResources.T("ProfileMedicalInformation"),
                    Subtitle = AppResources.T("ProfileMedicalInformationHint"),
                    IconGlyph = "❤️",
                    TapCommand = new Command(async () => await ComingSoonAsync()),
                },
                new ProfileMenuRowModel
                {
                    Title = AppResources.T("ProfileEmergencyContacts"),
                    Subtitle = AppResources.T("ProfileEmergencyContactsHint"),
                    IconGlyph = "📞",
                    TapCommand = new Command(async () => await ComingSoonAsync()),
                },
                new ProfileMenuRowModel
                {
                    Title = AppResources.HomeHubFamily,
                    Subtitle = AppResources.T("ProfileFamilyMembersHint"),
                    IconGlyph = "👨‍👩‍👧",
                    ShowSeparator = false,
                    TapCommand = new Command(async () =>
                        await AppNavigator.GoToFeatureAsync(AppNavigator.FamilyMembers, AppResources.HomeHubFamily)),
                },
            ]),
        });

        Sections.Add(new ProfileSectionModel
        {
            Title = AppResources.T("ProfileSectionInsurance"),
            Items = new ObservableCollection<ProfileMenuRowModel>(
            [
                new ProfileMenuRowModel
                {
                    Title = AppResources.T("ProfileCurrentPlan"),
                    Subtitle = AppResources.T("ProfileCurrentPlanHint"),
                    IconGlyph = "🛡️",
                    TapCommand = new Command(async () =>
                        await AppNavigator.GoToFeatureAsync(AppNavigator.Insurance, AppResources.HomeHubInsurance)),
                },
                new ProfileMenuRowModel
                {
                    Title = AppResources.T("ProfilePaymentMethods"),
                    Subtitle = AppResources.T("ProfilePaymentMethodsHint"),
                    IconGlyph = "💳",
                    TapCommand = new Command(async () =>
                        await AppNavigator.GoToFeatureAsync(AppNavigator.Billing, AppResources.HomeHubBilling)),
                },
                new ProfileMenuRowModel
                {
                    Title = AppResources.T("ProfileBillingHistory"),
                    Subtitle = AppResources.T("ProfileBillingHistoryHint"),
                    IconGlyph = "🧾",
                    ShowSeparator = false,
                    TapCommand = new Command(async () =>
                        await AppNavigator.GoToFeatureAsync(AppNavigator.Billing, AppResources.HomeHubBilling)),
                },
            ]),
        });

        Sections.Add(new ProfileSectionModel
        {
            Title = AppResources.T("ProfileSectionApp"),
            Items = new ObservableCollection<ProfileMenuRowModel>(
            [
                new ProfileMenuRowModel
                {
                    Title = AppResources.HomeHubNotifications,
                    Subtitle = AppResources.T("ProfileNotificationsHint"),
                    IconGlyph = "🔔",
                    TapCommand = new Command(async () =>
                        await AppNavigator.GoToFeatureAsync(AppNavigator.Notifications, AppResources.HomeHubNotifications)),
                },
                new ProfileMenuRowModel
                {
                    Title = AppResources.T("ProfilePrivacy"),
                    Subtitle = AppResources.T("ProfilePrivacyHint"),
                    IconGlyph = "👁️",
                    TapCommand = new Command(async () => await SafeShellNavigator.GoToAsync(AppNavigator.Privacy)),
                },
                new ProfileMenuRowModel
                {
                    Title = AppResources.T("ProfileHelpSupport"),
                    Subtitle = AppResources.T("ProfileHelpSupportHint"),
                    IconGlyph = "❔",
                    ShowSeparator = false,
                    TapCommand = new Command(async () => await SafeShellNavigator.GoToAsync(AppNavigator.HelpSupport)),
                },
            ]),
        });
    }

    private static string CurrentLanguageSubtitle() =>
        string.Format(AppResources.T("ProfileLanguageCurrentFormat"), CultureInfo.CurrentUICulture.NativeName);

    private async Task ComingSoonAsync() =>
        await MainThread.InvokeOnMainThreadAsync(async () =>
            await Shell.Current.DisplayAlertAsync(Title, AppResources.T("ProfileFeatureComingSoon"), AppResources.T("CommonOk")));

    private async Task LanguageHintAsync() =>
        await MainThread.InvokeOnMainThreadAsync(async () =>
            await Shell.Current.DisplayAlertAsync(
                AppResources.T("ProfileLanguage"),
                AppResources.T("ProfileLanguageHintBody"),
                AppResources.T("CommonOk")));

    private async Task SignOutAsync()
    {
        var confirm = await MainThread.InvokeOnMainThreadAsync(async () =>
            await Shell.Current.DisplayAlertAsync(
                AppResources.T("ProfileSignOutTitle"),
                AppResources.T("ProfileSignOutMessage"),
                AppResources.T("ProfileSignOutConfirm"),
                AppResources.T("CommonCancel")));
        if (!confirm)
            return;

        await _auth.SignOutAsync(CancellationToken.None);
        await SafeShellNavigator.GoToAsync("//" + AppNavigator.Landing);
    }
}
