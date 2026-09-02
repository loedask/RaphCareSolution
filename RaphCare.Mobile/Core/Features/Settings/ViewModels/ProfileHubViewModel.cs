using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Mobile.Core.Common.Icons;
using RaphCare.Mobile.Core.Features.Settings.Models;
using RaphCare.Mobile.Core.Features.Settings.Services;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.Services.Api;
using RaphCare.Mobile.Core.Common.Services.Auth;
using RaphCare.Mobile.Core.Common.Services.Localization;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Settings.ViewModels;

/// <summary>Profile hub (concept <c>Profile.tsx</c>): avatar, grouped links, sign out.</summary>
public sealed class ProfileHubViewModel : BaseViewModel
{
    private readonly IAuthService _auth;
    private readonly IPatientProfileService _profileApi;
    private readonly IPatientEmergencyContactsService _emergencyContactsApi;
    private readonly ILocalPatientProfileStore _localProfile;
    private readonly ISelectedClinicStore _selectedClinic;
    private int _emergencyContactCount;
    private string _displayName = string.Empty;
    private string _emailLine = string.Empty;
    private ImageSource? _profilePhoto;
    private bool _hasProfilePhoto;

    public ProfileHubViewModel(
        IAuthService auth,
        IPatientProfileService profileApi,
        IPatientEmergencyContactsService emergencyContactsApi,
        ILocalPatientProfileStore localProfile,
        ISelectedClinicStore selectedClinic)
    {
        _auth = auth ?? throw new ArgumentNullException(nameof(auth));
        _profileApi = profileApi ?? throw new ArgumentNullException(nameof(profileApi));
        _emergencyContactsApi = emergencyContactsApi ?? throw new ArgumentNullException(nameof(emergencyContactsApi));
        _localProfile = localProfile ?? throw new ArgumentNullException(nameof(localProfile));
        _selectedClinic = selectedClinic ?? throw new ArgumentNullException(nameof(selectedClinic));

        Title = T("ProfileHubTitle");
        EditProfileCommand = new Command(async () => await SafeShellNavigator.GoToAsync(AppNavigator.EditProfile));
        SignOutCommand = new Command(async () => await SignOutAsync());
        OpenPhotoCommand = new Command(async () => await PickAndUploadPhotoAsync());
        Sections = new ObservableCollection<ProfileSectionModel>();
        AppVersionLabel = $"{T("ProfileAppName")} v{AppInfo.Current.VersionString}";
        InsuranceBadgeText = T("ProfileInsuranceBadge");
        EditProfileButtonText = T("ProfileEditProfile");
        SignOutButtonText = T("ProfileSignOut");
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

    public ImageSource? ProfilePhoto
    {
        get => _profilePhoto;
        private set
        {
            SetProperty(ref _profilePhoto, value);
            HasProfilePhoto = value is not null;
        }
    }

    public bool HasProfilePhoto
    {
        get => _hasProfilePhoto;
        private set => SetProperty(ref _hasProfilePhoto, value);
    }

    public string InsuranceBadgeText { get; }
    public string AppVersionLabel { get; }

    public ObservableCollection<ProfileSectionModel> Sections { get; }

    public ICommand EditProfileCommand { get; }
    public ICommand SignOutCommand { get; }
    public ICommand OpenPhotoCommand { get; }

    public async Task LoadAsync()
    {
        var contactsTask = _emergencyContactsApi.GetMyEmergencyContactsAsync(CancellationToken.None);
        var response = await _profileApi.GetMyProfileAsync(CancellationToken.None).ConfigureAwait(false);

        var contactsResponse = await contactsTask.ConfigureAwait(false);
        if (contactsResponse.IsSuccess && contactsResponse.Data is not null)
            _emergencyContactCount = contactsResponse.Data.Count;
        else
            _emergencyContactCount = _localProfile.GetEmergencyContacts().Count;

        if (response.IsSuccess && response.Data is { } data)
        {
            _localProfile.FirstName = data.FirstName;
            _localProfile.LastName = data.LastName;
            _localProfile.Email = data.Email ?? string.Empty;
            _localProfile.Phone = data.PhoneNumber ?? string.Empty;
            _localProfile.DateOfBirth = data.DateOfBirth;
            _localProfile.Gender = data.Gender;
            await ApplyProfileDisplayAsync(data.FirstName, data.LastName, data.Email).ConfigureAwait(false);
            await LoadProfilePhotoAsync(!string.IsNullOrWhiteSpace(data.ProfilePhotoUrl)).ConfigureAwait(false);
            return;
        }

        var token = await _auth.GetAccessTokenAsync(CancellationToken.None).ConfigureAwait(false);
        var (claimName, claimEmail) = JwtClaimsReader.ReadDisplayClaims(token);

        var first = _localProfile.FirstName.Trim();
        var last = _localProfile.LastName.Trim();
        await ApplyProfileDisplayAsync(first, last, _localProfile.Email.Trim(), claimName, claimEmail).ConfigureAwait(false);
        await LoadProfilePhotoAsync(false).ConfigureAwait(false);
    }

    private async Task LoadProfilePhotoAsync(bool mayHavePhoto)
    {
        if (!mayHavePhoto)
        {
            await MainThread.InvokeOnMainThreadAsync(() => ProfilePhoto = null);
            return;
        }

        var photoResponse = await _profileApi.GetProfilePhotoBytesAsync(CancellationToken.None).ConfigureAwait(false);
        if (!photoResponse.IsSuccess || photoResponse.Data is not { Length: > 0 } bytes)
        {
            await MainThread.InvokeOnMainThreadAsync(() => ProfilePhoto = null);
            return;
        }

        await MainThread.InvokeOnMainThreadAsync(() =>
            ProfilePhoto = ImageSource.FromStream(() => new MemoryStream(bytes)));
    }

    private async Task PickAndUploadPhotoAsync()
    {
        FileResult? picked = null;
        try
        {
            var results = await MainThread.InvokeOnMainThreadAsync(() =>
                MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions { Title = T("ProfilePhotoPickerTitle") }));
            picked = results is { Count: > 0 } ? results[0] : null;
        }
        catch (FeatureNotSupportedException)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.DisplayAlertAsync(Title, T("ProfilePhotoComingSoon"), T("CommonOk")));
            return;
        }

        if (picked is null)
            return;

        await using var stream = await picked.OpenReadAsync();
        var response = await _profileApi.UploadProfilePhotoAsync(
            stream,
            picked.FileName,
            picked.ContentType ?? "image/jpeg",
            CancellationToken.None).ConfigureAwait(false);

        if (!response.IsSuccess)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.DisplayAlertAsync(Title, T("ProfilePhotoUploadFailed"), T("CommonOk")));
            return;
        }

        await LoadProfilePhotoAsync(true);
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
            : (!string.IsNullOrEmpty(claimNameFallback) ? claimNameFallback : T("ProfileDefaultName"));

        var email = profileEmail?.Trim() ?? string.Empty;
        EmailLine = !string.IsNullOrEmpty(email)
            ? email
            : (!string.IsNullOrEmpty(claimEmailFallback) ? claimEmailFallback : T("ProfileNoEmail"));
    }

    private void BuildSections()
    {
        Sections.Clear();

        Sections.Add(new ProfileSectionModel
        {
            Title = T("ProfileSectionAccount"),
            Items = new ObservableCollection<ProfileMenuRowModel>(
            [
                new ProfileMenuRowModel
                {
                    Title = T("ProfilePersonalInformation"),
                    Subtitle = T("ProfilePersonalInformationHint"),
                    IconSource = MonochromeIconKeys.User,
                    TapCommand = new Command(async () => await SafeShellNavigator.GoToAsync(AppNavigator.PersonalInformation)),
                },
                new ProfileMenuRowModel
                {
                    Title = T("ProfileMyClinic"),
                    Subtitle = ClinicSubtitle(),
                    IconSource = MonochromeIconKeys.Hospital,
                    TapCommand = new Command(async () => await SafeShellNavigator.GoToAsync(AppNavigator.SelectClinic)),
                },
                new ProfileMenuRowModel
                {
                    Title = T("ProfileChangePassword"),
                    Subtitle = T("ProfileChangePasswordHint"),
                    IconSource = MonochromeIconKeys.Lock,
                    TapCommand = new Command(async () => await SafeShellNavigator.GoToAsync(AppNavigator.ChangePassword)),
                },
                new ProfileMenuRowModel
                {
                    Title = T("ProfileLanguage"),
                    Subtitle = CurrentLanguageSubtitle(),
                    IconSource = MonochromeIconKeys.Globe,
                    ShowSeparator = false,
                    TapCommand = new Command(async () => await SafeShellNavigator.GoToAsync(AppNavigator.LanguageSettings)),
                },
            ]),
        });

        Sections.Add(new ProfileSectionModel
        {
            Title = T("ProfileSectionHealth"),
            Items = new ObservableCollection<ProfileMenuRowModel>(
            [
                new ProfileMenuRowModel
                {
                    Title = T("ProfileMedicalInformation"),
                    Subtitle = T("ProfileMedicalInformationHint"),
                    IconSource = MonochromeIconKeys.Heart,
                    TapCommand = new Command(async () => await SafeShellNavigator.GoToAsync(AppNavigator.MedicalInformation)),
                },
                new ProfileMenuRowModel
                {
                    Title = T("ProfileEmergencyContacts"),
                    Subtitle = EmergencyContactsSubtitle(),
                    IconSource = MonochromeIconKeys.Phone,
                    TapCommand = new Command(async () => await SafeShellNavigator.GoToAsync(AppNavigator.EmergencyContacts)),
                },
                new ProfileMenuRowModel
                {
                    Title = T("HomeHubFamily"),
                    Subtitle = T("ProfileFamilyMembersHint"),
                    IconSource = MonochromeIconKeys.Users,
                    ShowSeparator = false,
                    TapCommand = new Command(async () =>
                        await AppNavigator.GoToFeatureAsync(AppNavigator.FamilyMembers, T("HomeHubFamily"))),
                },
            ]),
        });

        Sections.Add(new ProfileSectionModel
        {
            Title = T("ProfileSectionInsurance"),
            Items = new ObservableCollection<ProfileMenuRowModel>(
            [
                new ProfileMenuRowModel
                {
                    Title = T("ProfileCurrentPlan"),
                    Subtitle = T("ProfileCurrentPlanHint"),
                    IconSource = MonochromeIconKeys.Shield,
                    TapCommand = new Command(async () =>
                        await AppNavigator.GoToFeatureAsync(AppNavigator.Insurance, T("HomeHubInsurance"))),
                },
                new ProfileMenuRowModel
                {
                    Title = T("ProfilePaymentMethods"),
                    Subtitle = T("ProfilePaymentMethodsHint"),
                    IconSource = MonochromeIconKeys.CreditCard,
                    TapCommand = new Command(async () =>
                        await AppNavigator.GoToFeatureAsync(AppNavigator.Billing, T("HomeHubBilling"))),
                },
                new ProfileMenuRowModel
                {
                    Title = T("ProfileBillingHistory"),
                    Subtitle = T("ProfileBillingHistoryHint"),
                    IconSource = MonochromeIconKeys.Receipt,
                    ShowSeparator = false,
                    TapCommand = new Command(async () =>
                        await AppNavigator.GoToFeatureAsync(AppNavigator.Billing, T("HomeHubBilling"))),
                },
            ]),
        });

        Sections.Add(new ProfileSectionModel
        {
            Title = T("ProfileSectionApp"),
            Items = new ObservableCollection<ProfileMenuRowModel>(
            [
                new ProfileMenuRowModel
                {
                    Title = T("HomeHubNotifications"),
                    Subtitle = T("ProfileNotificationsHint"),
                    IconSource = MonochromeIconKeys.Bell,
                    TapCommand = new Command(async () =>
                        await AppNavigator.GoToFeatureAsync(AppNavigator.Notifications, T("HomeHubNotifications"))),
                },
                new ProfileMenuRowModel
                {
                    Title = T("ProfilePrivacy"),
                    Subtitle = T("ProfilePrivacyHint"),
                    IconSource = MonochromeIconKeys.Eye,
                    TapCommand = new Command(async () => await SafeShellNavigator.GoToAsync(AppNavigator.Privacy)),
                },
                new ProfileMenuRowModel
                {
                    Title = T("ProfileHelpSupport"),
                    Subtitle = T("ProfileHelpSupportHint"),
                    IconSource = MonochromeIconKeys.Help,
                    ShowSeparator = false,
                    TapCommand = new Command(async () => await SafeShellNavigator.GoToAsync(AppNavigator.HelpSupport)),
                },
            ]),
        });
    }

    private string ClinicSubtitle()
    {
        if (_selectedClinic.ClinicId is null)
            return T("ProfileMyClinicHint");

        var name = _selectedClinic.ClinicName ?? T("SelectClinicUnknownName");
        return string.IsNullOrWhiteSpace(_selectedClinic.ReferenceCode)
            ? name
            : Format(T("ProfileMyClinicSelectedFormat"), name, _selectedClinic.ReferenceCode);
    }

    private static string CurrentLanguageSubtitle() => AppLanguagePreference.CurrentNativeName;

    private string EmergencyContactsSubtitle()
    {
        var count = _emergencyContactCount;
        return count == 0
            ? T("ProfileEmergencyContactsHint")
            : Format(T("ProfileEmergencyContactsCountFormat"), count);
    }

    private async Task SignOutAsync()
    {
        var confirm = await MainThread.InvokeOnMainThreadAsync(async () =>
            await Shell.Current.DisplayAlertAsync(
                T("ProfileSignOutTitle"),
                T("ProfileSignOutMessage"),
                T("ProfileSignOutConfirm"),
                T("CommonCancel")));
        if (!confirm)
            return;

        await _auth.SignOutAsync(CancellationToken.None);
        await SafeShellNavigator.GoToAsync($"//{AppNavigator.Landing}");
    }
}
