using System.Windows.Input;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.MedicalInfo;
using RaphCare.Mobile.Core.Features.Settings.Services;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Settings.ViewModels;

/// <summary>Medical summary (concept <c>MedicalInformation.tsx</c>). Synced via <c>api/patient/medical-info</c>.</summary>
public sealed class MedicalInformationViewModel : BaseViewModel
{
    private readonly IPatientMedicalInfoService _api;
    private readonly ILocalPatientProfileStore _local;
    private string _bloodType = string.Empty;
    private string _allergies = string.Empty;
    private string _chronicConditions = string.Empty;
    private string _medications = string.Empty;
    private string _primaryDoctor = string.Empty;
    private int _bloodTypeIndex = -1;

    public MedicalInformationViewModel(IPatientMedicalInfoService api, ILocalPatientProfileStore local)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
        _local = local ?? throw new ArgumentNullException(nameof(local));
        Title = T("MedicalInformationTitle");
        BloodTypeLabel = T("MedicalInformationBloodType");
        AllergiesLabel = T("MedicalInformationAllergies");
        ChronicLabel = T("MedicalInformationChronic");
        MedicationsLabel = T("MedicalInformationMedications");
        PrimaryDoctorLabel = T("MedicalInformationPrimaryDoctor");
        SaveLabel = T("EditProfileSave");
        BloodTypeOptions = ["A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"];
        SaveCommand = new Command(async () => await SaveAsync());
    }

    public string BloodTypeLabel { get; }
    public string AllergiesLabel { get; }
    public string ChronicLabel { get; }
    public string MedicationsLabel { get; }
    public string PrimaryDoctorLabel { get; }
    public string SaveLabel { get; }
    public IReadOnlyList<string> BloodTypeOptions { get; }

    public int BloodTypeIndex
    {
        get => _bloodTypeIndex;
        set
        {
            SetProperty(ref _bloodTypeIndex, value);
            if (value >= 0 && value < BloodTypeOptions.Count)
                BloodType = BloodTypeOptions[value];
        }
    }

    public string BloodType
    {
        get => _bloodType;
        set => SetProperty(ref _bloodType, value);
    }

    public string Allergies
    {
        get => _allergies;
        set => SetProperty(ref _allergies, value);
    }

    public string ChronicConditions
    {
        get => _chronicConditions;
        set => SetProperty(ref _chronicConditions, value);
    }

    public string Medications
    {
        get => _medications;
        set => SetProperty(ref _medications, value);
    }

    public string PrimaryDoctor
    {
        get => _primaryDoctor;
        set => SetProperty(ref _primaryDoctor, value);
    }

    public ICommand SaveCommand { get; }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var response = await _api.GetMyMedicalInfoAsync(CancellationToken.None).ConfigureAwait(false);
            if (response.IsSuccess && response.Data is { } data)
            {
                ApplyFromApi(data);
                CopyToLocal(data);
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
        BloodType = _local.BloodType;
        BloodTypeIndex = BloodTypeOptions.ToList().IndexOf(BloodType);
        Allergies = _local.Allergies;
        ChronicConditions = _local.ChronicConditions;
        Medications = _local.Medications;
        PrimaryDoctor = _local.PrimaryDoctor;
    }

    private void ApplyFromApi(MyPatientMedicalInfoViewModel data)
    {
        BloodType = data.BloodType;
        BloodTypeIndex = BloodTypeOptions.ToList().IndexOf(BloodType);
        Allergies = data.Allergies;
        ChronicConditions = data.ChronicConditions;
        Medications = data.Medications;
        PrimaryDoctor = data.PrimaryDoctor;
    }

    private async Task SaveAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var request = new MyPatientMedicalInfoUpdateRequest
            {
                BloodType = BloodType.Trim(),
                Allergies = Allergies.Trim(),
                ChronicConditions = ChronicConditions.Trim(),
                Medications = Medications.Trim(),
                PrimaryDoctor = PrimaryDoctor.Trim(),
            };

            var response = await _api.UpdateMyMedicalInfoAsync(request, CancellationToken.None).ConfigureAwait(false);
            if (!response.IsSuccess)
            {
                await DisplayAlertSafeAsync(Title, T("MedicalInformationSaveFailed"), T("CommonOk"));
                return;
            }

            CopyToLocal(request);

            await DisplayAlertSafeAsync(Title, T("MedicalInformationSaved"), T("CommonOk"));
            await SafeShellNavigator.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void CopyToLocal(MyPatientMedicalInfoViewModel data)
    {
        _local.BloodType = data.BloodType;
        _local.Allergies = data.Allergies;
        _local.ChronicConditions = data.ChronicConditions;
        _local.Medications = data.Medications;
        _local.PrimaryDoctor = data.PrimaryDoctor;
    }

    private void CopyToLocal(MyPatientMedicalInfoUpdateRequest data) => CopyToLocal(new MyPatientMedicalInfoViewModel
    {
        BloodType = data.BloodType,
        Allergies = data.Allergies,
        ChronicConditions = data.ChronicConditions,
        Medications = data.Medications,
        PrimaryDoctor = data.PrimaryDoctor,
    });
}
