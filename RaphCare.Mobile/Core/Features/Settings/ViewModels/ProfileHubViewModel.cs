using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
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
    private readonly ILocalPatientProfileStore _profile;
    private string _displayName = string.Empty;
    private string _emailLine = string.Empty;

    public ProfileHubViewModel(IAuthService auth, ILocalPatientProfileStore profile)
    {
        _auth = auth ?? throw new ArgumentNullException(nameof(auth));
        _profile = profile ?? throw new ArgumentNullException(nameof(profile));

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
        var token = await _auth.GetAccessTokenAsync(CancellationToken.None).ConfigureAwait(false);
        var (claimName, claimEmail) = JwtClaimsReader.ReadDisplayClaims(token);

        var first = _profile.FirstName.Trim();
        var last = _profile.LastName.Trim();
        var combined = string.Join(" ", new[] { first, last }.Where(s => s.Length > 0));
        DisplayName = !string.IsNullOrEmpty(combined)
            ? combined
            : (!string.IsNullOrEmpty(claimName) ? claimName : AppResources.T("ProfileDefaultName"));

        var email = _profile.Email.Trim();
        EmailLine = !string.IsNullOrEmpty(email)
            ? email
            : (!string.IsNullOrEmpty(claimEmail) ? claimEmail : AppResources.T("ProfileNoEmail"));

        BuildSections();
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

        await _auth.SignOutAsync(CancellationToken.None).ConfigureAwait(false);
        await SafeShellNavigator.GoToAsync("//" + AppNavigator.Landing);
    }
}
