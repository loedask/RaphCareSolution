using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using RaphCare.Mobile.Core.Features.Settings.Services;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Settings.ViewModels;

/// <summary>Medical summary (concept <c>MedicalInformation.tsx</c>). Stored locally until clinical API expands.</summary>
public sealed class MedicalInformationViewModel : BaseViewModel
{
    private readonly ILocalPatientProfileStore _local;
    private string _bloodType = string.Empty;
    private string _allergies = string.Empty;
    private string _chronicConditions = string.Empty;
    private string _medications = string.Empty;
    private string _primaryDoctor = string.Empty;
    private int _bloodTypeIndex = -1;

    public MedicalInformationViewModel(ILocalPatientProfileStore local)
    {
        _local = local;
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

    public void LoadFromStore()
    {
        BloodType = _local.BloodType;
        BloodTypeIndex = BloodTypeOptions.ToList().IndexOf(BloodType);
        Allergies = _local.Allergies;
        ChronicConditions = _local.ChronicConditions;
        Medications = _local.Medications;
        PrimaryDoctor = _local.PrimaryDoctor;
    }

    private async Task SaveAsync()
    {
        _local.BloodType = BloodType.Trim();
        _local.Allergies = Allergies.Trim();
        _local.ChronicConditions = ChronicConditions.Trim();
        _local.Medications = Medications.Trim();
        _local.PrimaryDoctor = PrimaryDoctor.Trim();

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Shell.Current.DisplayAlertAsync(Title, T("MedicalInformationSaved"), T("CommonOk"));
            await Shell.Current.GoToAsync("..");
        });
    }
}
