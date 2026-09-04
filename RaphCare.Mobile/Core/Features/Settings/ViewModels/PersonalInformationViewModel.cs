using System.Windows.Input;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Profile;
using RaphCare.Mobile.Core.Features.Settings.Services;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Settings.ViewModels;

/// <summary>Personal information (concept <c>PersonalInformation.tsx</c>).</summary>
public sealed class PersonalInformationViewModel : BaseViewModel
{
    private readonly IPatientProfileService _profileApi;
    private readonly ILocalPatientProfileStore _local;
    private string _fullName = string.Empty;
    private string _address = string.Empty;
    private string _phone = string.Empty;
    private DateTime _dateOfBirth = DateTime.Today.AddYears(-30);
    private int _genderIndex;

    public PersonalInformationViewModel(IPatientProfileService profileApi, ILocalPatientProfileStore local)
    {
        _profileApi = profileApi;
        _local = local;
        Title = T("PersonalInformationTitle");
        FullNameLabel = T("PersonalInformationFullName");
        DobLabel = T("EditProfileDob");
        GenderLabel = T("EditProfileGender");
        AddressLabel = T("PersonalInformationAddress");
        PhoneLabel = T("EditProfilePhone");
        SaveLabel = T("EditProfileSave");
        GenderOptions =
        [
            T("ProfileGenderFemale"),
            T("ProfileGenderMale"),
            T("ProfileGenderOther"),
            T("ProfileGenderPreferNot"),
        ];
        SaveCommand = new Command(async () => await SaveAsync());
    }

    public string FullNameLabel { get; }
    public string DobLabel { get; }
    public string GenderLabel { get; }
    public string AddressLabel { get; }
    public string PhoneLabel { get; }
    public string SaveLabel { get; }
    public IReadOnlyList<string> GenderOptions { get; }

    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
    }

    public string Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    public string Phone
    {
        get => _phone;
        set => SetProperty(ref _phone, value);
    }

    public DateTime DateOfBirth
    {
        get => _dateOfBirth;
        set => SetProperty(ref _dateOfBirth, value);
    }

    public int GenderIndex
    {
        get => _genderIndex;
        set => SetProperty(ref _genderIndex, value);
    }

    public ICommand SaveCommand { get; }

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
                _local.Address = Address;
                return;
            }

            LoadFromLocal();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void LoadFromLocal()
    {
        FullName = string.Join(" ", new[] { _local.FirstName.Trim(), _local.LastName.Trim() }.Where(s => s.Length > 0));
        Phone = _local.Phone;
        DateOfBirth = _local.DateOfBirth ?? DateTime.Today.AddYears(-25);
        GenderIndex = MapGenderToIndex(_local.Gender);
        Address = _local.Address;
    }

    private void ApplyFromApi(MyPatientProfileViewModel data)
    {
        FullName = string.Join(" ", new[] { data.FirstName.Trim(), data.LastName.Trim() }.Where(s => s.Length > 0));
        Phone = data.PhoneNumber ?? string.Empty;
        DateOfBirth = data.DateOfBirth;
        GenderIndex = MapGenderToIndex(data.Gender);
        Address = _local.Address;
    }

    private async Task SaveAsync()
    {
        if (IsBusy) return;
        var parts = FullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var first = parts.Length > 0 ? parts[0] : string.Empty;
        var last = parts.Length > 1 ? parts[1] : string.Empty;

        IsBusy = true;
        try
        {
            var request = new MyPatientProfileUpdateRequest
            {
                FirstName = first,
                LastName = last,
                Email = string.IsNullOrWhiteSpace(_local.Email) ? null : _local.Email.Trim(),
                PhoneNumber = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim(),
                DateOfBirth = DateOfBirth.Date,
                Gender = MapIndexToGender(GenderIndex),
            };

            var response = await _profileApi.UpdateMyProfileAsync(request, CancellationToken.None).ConfigureAwait(false);
            if (!response.IsSuccess)
            {
                await DisplayAlertSafeAsync(T("PersonalInformationTitle"), T("PersonalInformationSaveFailed"), T("CommonOk"));
                return;
            }

            _local.FirstName = first;
            _local.LastName = last;
            _local.Phone = Phone.Trim();
            _local.DateOfBirth = DateOfBirth.Date;
            _local.Gender = request.Gender;
            _local.Address = Address.Trim();

            await DisplayAlertSafeAsync(T("PersonalInformationTitle"), T("PersonalInformationSaved"), T("CommonOk"));
            await SafeShellNavigator.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static int MapGenderToIndex(string stored) =>
        stored switch
        {
            "male" => 1,
            "other" => 2,
            "prefer-not" => 3,
            _ => 0,
        };

    private static string MapIndexToGender(int index) =>
        index switch
        {
            1 => "male",
            2 => "other",
            3 => "prefer-not",
            _ => "female",
        };
}
