using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Profile;
using RaphCare.Mobile.Core.Features.Settings.Services;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Settings.ViewModels;

/// <summary>Edit profile (concept <c>EditProfile.tsx</c>). Loads/saves via <c>api/patient/profile</c>; local store is a cache.</summary>
public sealed class EditProfileViewModel : BaseViewModel
{
    private readonly IPatientProfileService _profileApi;
    private readonly ILocalPatientProfileStore _localProfile;
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _email = string.Empty;
    private string _phone = string.Empty;
    private DateTime _dob = DateTime.Today.AddYears(-30);
    private int _genderIndex;
    private ImageSource? _profilePhoto;
    private bool _hasProfilePhoto;
    private bool _isUploadingPhoto;
    private bool _mayHavePhoto;

    public EditProfileViewModel(IPatientProfileService profileApi, ILocalPatientProfileStore localProfile)
    {
        _profileApi = profileApi ?? throw new ArgumentNullException(nameof(profileApi));
        _localProfile = localProfile ?? throw new ArgumentNullException(nameof(localProfile));
        Title = T("EditProfileTitle");

        PhotoHint = T("EditProfilePhotoHint");
        SaveCommand = new Command(async () => await SaveAsync());
        OpenPhotoCommand = new Command(async () => await PickAndUploadPhotoAsync());
        GenderOptions =
        [
            T("ProfileGenderFemale"),
            T("ProfileGenderMale"),
            T("ProfileGenderOther"),
            T("ProfileGenderPreferNot"),
        ];
        FirstNameLabel = T("EditProfileFirstName");
        LastNameLabel = T("EditProfileLastName");
        EmailLabel = T("EditProfileEmail");
        PhoneLabel = T("EditProfilePhone");
        DobLabel = T("EditProfileDob");
        GenderLabel = T("EditProfileGender");
        SaveLabel = T("EditProfileSave");
    }

    public string FirstNameLabel { get; }
    public string LastNameLabel { get; }
    public string EmailLabel { get; }
    public string PhoneLabel { get; }
    public string DobLabel { get; }
    public string GenderLabel { get; }
    public string SaveLabel { get; }
    public string PhotoHint { get; }

    public IReadOnlyList<string> GenderOptions { get; }

    public string FirstName
    {
        get => _firstName;
        set => SetProperty(ref _firstName, value);
    }

    public string LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Phone
    {
        get => _phone;
        set => SetProperty(ref _phone, value);
    }

    public DateTime DateOfBirth
    {
        get => _dob;
        set => SetProperty(ref _dob, value);
    }

    /// <summary>Index into <see cref="GenderOptions"/>.</summary>
    public int GenderIndex
    {
        get => _genderIndex;
        set => SetProperty(ref _genderIndex, value);
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

    public bool IsUploadingPhoto
    {
        get => _isUploadingPhoto;
        private set => SetProperty(ref _isUploadingPhoto, value);
    }

    public ICommand SaveCommand { get; }
    public ICommand OpenPhotoCommand { get; }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var response = await _profileApi.GetMyProfileAsync(CancellationToken.None).ConfigureAwait(false);
            if (response.IsSuccess && response.Data is { } data)
            {
                ApplyFromApi(data);
                CopyToLocalStore(data);
                _mayHavePhoto = !string.IsNullOrWhiteSpace(data.ProfilePhotoUrl);
                await LoadProfilePhotoAsync(_mayHavePhoto).ConfigureAwait(false);
                return;
            }

            LoadFromLocalStore();
            await LoadProfilePhotoAsync(false).ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void LoadFromLocalStore()
    {
        FirstName = _localProfile.FirstName;
        LastName = _localProfile.LastName;
        Email = _localProfile.Email;
        Phone = _localProfile.Phone;
        DateOfBirth = _localProfile.DateOfBirth ?? DateTime.Today.AddYears(-25);
        GenderIndex = MapGenderToIndex(_localProfile.Gender);
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
        if (IsUploadingPhoto)
            return;

        FileResult? picked = null;
        try
        {
            var results = await MainThread.InvokeOnMainThreadAsync(() =>
                MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions { Title = T("ProfilePhotoPickerTitle") }));
            picked = results is { Count: > 0 } ? results[0] : null;
        }
        catch (FeatureNotSupportedException)
        {
            await DisplayAlertSafeAsync(Title, T("ProfilePhotoComingSoon"), T("CommonOk"));
            return;
        }

        if (picked is null)
            return;

        IsUploadingPhoto = true;
        try
        {
            await using var stream = await picked.OpenReadAsync();
            var response = await _profileApi.UploadProfilePhotoAsync(
                stream,
                picked.FileName,
                picked.ContentType ?? "image/jpeg",
                CancellationToken.None).ConfigureAwait(false);

            if (!response.IsSuccess)
            {
                await DisplayAlertSafeAsync(Title, T("ProfilePhotoUploadFailed"), T("CommonOk"));
                return;
            }

            _mayHavePhoto = true;
            await LoadProfilePhotoAsync(true).ConfigureAwait(false);
        }
        finally
        {
            IsUploadingPhoto = false;
        }
    }

    private async Task SaveAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var request = new MyPatientProfileUpdateRequest
            {
                FirstName = FirstName.Trim(),
                LastName = LastName.Trim(),
                Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
                PhoneNumber = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim(),
                DateOfBirth = DateOfBirth.Date,
                Gender = MapIndexToGender(GenderIndex),
            };

            var response = await _profileApi.UpdateMyProfileAsync(request, CancellationToken.None).ConfigureAwait(false);
            if (!response.IsSuccess)
            {
                await DisplayAlertSafeAsync(
                    T("EditProfileTitle"),
                    T("EditProfileSaveFailed"),
                    T("CommonOk"));
                return;
            }

            CopyToLocalStore(request);

            await DisplayAlertSafeAsync(
                T("EditProfileTitle"),
                T("EditProfileSaved"),
                T("CommonOk"));
            await SafeShellNavigator.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFromApi(MyPatientProfileViewModel data)
    {
        FirstName = data.FirstName;
        LastName = data.LastName;
        Email = data.Email ?? string.Empty;
        Phone = data.PhoneNumber ?? string.Empty;
        DateOfBirth = data.DateOfBirth;
        GenderIndex = MapGenderToIndex(data.Gender);
    }

    private void CopyToLocalStore(MyPatientProfileViewModel data)
    {
        _localProfile.FirstName = data.FirstName;
        _localProfile.LastName = data.LastName;
        _localProfile.Email = data.Email ?? string.Empty;
        _localProfile.Phone = data.PhoneNumber ?? string.Empty;
        _localProfile.DateOfBirth = data.DateOfBirth;
        _localProfile.Gender = data.Gender;
    }

    private void CopyToLocalStore(MyPatientProfileUpdateRequest data)
    {
        _localProfile.FirstName = data.FirstName;
        _localProfile.LastName = data.LastName;
        _localProfile.Email = data.Email ?? string.Empty;
        _localProfile.Phone = data.PhoneNumber ?? string.Empty;
        _localProfile.DateOfBirth = data.DateOfBirth;
        _localProfile.Gender = data.Gender;
    }

    private static int MapGenderToIndex(string stored)
    {
        return stored switch
        {
            "male" => 1,
            "other" => 2,
            "prefer-not" => 3,
            _ => 0,
        };
    }

    private static string MapIndexToGender(int index) =>
        index switch
        {
            1 => "male",
            2 => "other",
            3 => "prefer-not",
            _ => "female",
        };
}
