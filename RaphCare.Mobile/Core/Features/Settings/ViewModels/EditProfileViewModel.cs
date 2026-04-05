using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using RaphCare.Mobile.Core.Features.Settings.Services;
using RaphCare.Mobile.Core.Shared.ViewModels;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Features.Settings.ViewModels;

/// <summary>Edit profile (concept <c>EditProfile.tsx</c>). Persists to <see cref="ILocalPatientProfileStore"/>.</summary>
public sealed class EditProfileViewModel : BaseViewModel
{
    private readonly ILocalPatientProfileStore _profile;
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _email = string.Empty;
    private string _phone = string.Empty;
    private DateTime _dob = DateTime.Today.AddYears(-30);
    private int _genderIndex;

    public EditProfileViewModel(ILocalPatientProfileStore profile)
    {
        _profile = profile ?? throw new ArgumentNullException(nameof(profile));
        Title = AppResources.T("EditProfileTitle");
        SaveCommand = new Command(async () => await SaveAsync());
        GenderOptions =
        [
            AppResources.T("ProfileGenderFemale"),
            AppResources.T("ProfileGenderMale"),
            AppResources.T("ProfileGenderOther"),
            AppResources.T("ProfileGenderPreferNot"),
        ];
        FirstNameLabel = AppResources.T("EditProfileFirstName");
        LastNameLabel = AppResources.T("EditProfileLastName");
        EmailLabel = AppResources.T("EditProfileEmail");
        PhoneLabel = AppResources.T("EditProfilePhone");
        DobLabel = AppResources.T("EditProfileDob");
        GenderLabel = AppResources.T("EditProfileGender");
        SaveLabel = AppResources.T("EditProfileSave");
    }

    public string FirstNameLabel { get; }
    public string LastNameLabel { get; }
    public string EmailLabel { get; }
    public string PhoneLabel { get; }
    public string DobLabel { get; }
    public string GenderLabel { get; }
    public string SaveLabel { get; }

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

    public string PhotoHint => AppResources.T("EditProfilePhotoHint");
    public ICommand SaveCommand { get; }

    public void LoadFromStore()
    {
        FirstName = _profile.FirstName;
        LastName = _profile.LastName;
        Email = _profile.Email;
        Phone = _profile.Phone;
        DateOfBirth = _profile.DateOfBirth ?? DateTime.Today.AddYears(-25);
        GenderIndex = MapGenderToIndex(_profile.Gender);
    }

    private async Task SaveAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            _profile.FirstName = FirstName.Trim();
            _profile.LastName = LastName.Trim();
            _profile.Email = Email.Trim();
            _profile.Phone = Phone.Trim();
            _profile.DateOfBirth = DateOfBirth.Date;
            _profile.Gender = MapIndexToGender(GenderIndex);

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Shell.Current.DisplayAlertAsync(
                    AppResources.T("EditProfileTitle"),
                    AppResources.T("EditProfileSaved"),
                    AppResources.T("CommonOk"));
                await Shell.Current.GoToAsync("..");
            });
        }
        finally
        {
            IsBusy = false;
        }
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

    private string MapIndexToGender(int index) =>
        index switch
        {
            1 => "male",
            2 => "other",
            3 => "prefer-not",
            _ => "female",
        };
}
