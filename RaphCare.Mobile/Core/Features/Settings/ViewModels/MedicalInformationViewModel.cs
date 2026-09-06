using System.Collections.ObjectModel;
using System.Windows.Input;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.MedicalInfo;
using RaphCare.Mobile.Core.Common.MedicalInfo;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;
using RaphCare.Mobile.Core.Features.Settings.Models;
using RaphCare.Mobile.Core.Features.Settings.Services;

namespace RaphCare.Mobile.Core.Features.Settings.ViewModels;

/// <summary>Medical summary (concept <c>MedicalInformation.tsx</c>). Synced via <c>api/patient/medical-info</c>.</summary>
public sealed class MedicalInformationViewModel : BaseViewModel
{
    public const string BloodTypeUnknownStored = "Unknown";

    private static readonly string[] AllergyNoneAliases =
    [
        "NKDA",
        "NKA",
        "no known drug allergies",
        "no known allergies",
        "none known",
        "n/a",
    ];

    private readonly IPatientMedicalInfoService _api;
    private readonly ILocalPatientProfileStore _local;
    private readonly IReadOnlyList<MedicalInfoChipSpec> _allergySpecs;
    private readonly IReadOnlyList<MedicalInfoChipSpec> _chronicSpecs;
    private string _bloodType = string.Empty;
    private string _allergiesOther = string.Empty;
    private string _chronicOther = string.Empty;
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
        OtherNotesLabel = T("MedicalInformationOtherNotes");
        OtherNotesPlaceholder = T("MedicalInformationOtherNotesPlaceholder");
        ChipHint = T("MedicalInformationChipHint");
        SaveLabel = T("EditProfileSave");

        BloodTypeUnknownLabel = T("MedicalInformationBloodTypeUnknown");
        BloodTypeOptions =
        [
            "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-",
            BloodTypeUnknownLabel,
        ];

        _allergySpecs =
        [
            new("penicillin", T("MedicalInformationChipPenicillin"), false),
            new("sulfa", T("MedicalInformationChipSulfa"), false),
            new("latex", T("MedicalInformationChipLatex"), false),
            new("peanuts", T("MedicalInformationChipPeanuts"), false),
            new("shellfish", T("MedicalInformationChipShellfish"), false),
            new("none", T("MedicalInformationChipNoKnownAllergies"), true),
        ];
        _chronicSpecs =
        [
            new("diabetes", T("MedicalInformationChipDiabetes"), false),
            new("hypertension", T("MedicalInformationChipHypertension"), false),
            new("asthma", T("MedicalInformationChipAsthma"), false),
            new("heart_disease", T("MedicalInformationChipHeartDisease"), false),
            new("none", T("MedicalInformationChipNoChronic"), true),
        ];

        AllergyChips = new ObservableCollection<MedicalInfoChipItem>(
            _allergySpecs.Select(spec => new MedicalInfoChipItem(spec.Id, spec.Label, spec.IsExclusive, ToggleAllergyChip)));
        ChronicChips = new ObservableCollection<MedicalInfoChipItem>(
            _chronicSpecs.Select(spec => new MedicalInfoChipItem(spec.Id, spec.Label, spec.IsExclusive, ToggleChronicChip)));

        SaveCommand = new Command(async () => await SaveAsync());
    }

    public string BloodTypeLabel { get; }
    public string AllergiesLabel { get; }
    public string ChronicLabel { get; }
    public string MedicationsLabel { get; }
    public string PrimaryDoctorLabel { get; }
    public string OtherNotesLabel { get; }
    public string OtherNotesPlaceholder { get; }
    public string ChipHint { get; }
    public string SaveLabel { get; }
    public string BloodTypeUnknownLabel { get; }
    public IReadOnlyList<string> BloodTypeOptions { get; }
    public ObservableCollection<MedicalInfoChipItem> AllergyChips { get; }
    public ObservableCollection<MedicalInfoChipItem> ChronicChips { get; }

    public int BloodTypeIndex
    {
        get => _bloodTypeIndex;
        set
        {
            SetProperty(ref _bloodTypeIndex, value);
            if (value < 0 || value >= BloodTypeOptions.Count)
                return;

            BloodType = value == BloodTypeOptions.Count - 1
                ? BloodTypeUnknownStored
                : BloodTypeOptions[value];
        }
    }

    public string BloodType
    {
        get => _bloodType;
        set => SetProperty(ref _bloodType, value);
    }

    public string AllergiesOther
    {
        get => _allergiesOther;
        set => SetProperty(ref _allergiesOther, value ?? string.Empty);
    }

    public string ChronicOther
    {
        get => _chronicOther;
        set => SetProperty(ref _chronicOther, value ?? string.Empty);
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
        ApplyBloodType(_local.BloodType);
        ApplyAllergyField(_local.Allergies);
        ApplyChronicField(_local.ChronicConditions);
        Medications = _local.Medications;
        PrimaryDoctor = _local.PrimaryDoctor;
    }

    private void ApplyFromApi(MyPatientMedicalInfoViewModel data)
    {
        ApplyBloodType(data.BloodType);
        ApplyAllergyField(data.Allergies);
        ApplyChronicField(data.ChronicConditions);
        Medications = data.Medications;
        PrimaryDoctor = data.PrimaryDoctor;
    }

    private void ApplyBloodType(string? value)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        _bloodType = trimmed;

        if (string.IsNullOrEmpty(trimmed))
        {
            _bloodTypeIndex = -1;
        }
        else if (string.Equals(trimmed, BloodTypeUnknownStored, StringComparison.OrdinalIgnoreCase)
                 || string.Equals(trimmed, BloodTypeUnknownLabel, StringComparison.OrdinalIgnoreCase))
        {
            _bloodType = BloodTypeUnknownStored;
            _bloodTypeIndex = BloodTypeOptions.Count - 1;
        }
        else
        {
            _bloodTypeIndex = BloodTypeOptions.ToList().FindIndex(o =>
                string.Equals(o, trimmed, StringComparison.OrdinalIgnoreCase));
        }

        OnPropertyChanged(nameof(BloodType));
        OnPropertyChanged(nameof(BloodTypeIndex));
    }

    private void ApplyAllergyField(string? stored)
    {
        var (selected, other) = MedicalInfoChipComposer.Parse(stored, _allergySpecs, AllergyNoneAliases);
        ApplyChipSelection(AllergyChips, selected);
        AllergiesOther = other;
    }

    private void ApplyChronicField(string? stored)
    {
        var (selected, other) = MedicalInfoChipComposer.Parse(stored, _chronicSpecs);
        ApplyChipSelection(ChronicChips, selected);
        ChronicOther = other;
    }

    private static void ApplyChipSelection(
        IEnumerable<MedicalInfoChipItem> chips,
        IReadOnlySet<string> selectedIds)
    {
        foreach (var chip in chips)
            chip.IsSelected = selectedIds.Contains(chip.Id);
    }

    private void ToggleAllergyChip(MedicalInfoChipItem chip) =>
        ToggleChip(chip, AllergyChips, clearOther: () => AllergiesOther = string.Empty);

    private void ToggleChronicChip(MedicalInfoChipItem chip) =>
        ToggleChip(chip, ChronicChips, clearOther: () => ChronicOther = string.Empty);

    private static void ToggleChip(
        MedicalInfoChipItem chip,
        IEnumerable<MedicalInfoChipItem> group,
        Action clearOther)
    {
        if (chip.IsExclusive)
        {
            var turnOn = !chip.IsSelected;
            foreach (var item in group)
                item.IsSelected = false;
            if (turnOn)
            {
                chip.IsSelected = true;
                clearOther();
            }

            return;
        }

        chip.IsSelected = !chip.IsSelected;
        if (chip.IsSelected)
        {
            foreach (var item in group.Where(c => c.IsExclusive))
                item.IsSelected = false;
        }
    }

    private string ComposeAllergies() =>
        MedicalInfoChipComposer.Compose(
            _allergySpecs,
            AllergyChips.Where(c => c.IsSelected).Select(c => c.Id),
            AllergiesOther);

    private string ComposeChronic() =>
        MedicalInfoChipComposer.Compose(
            _chronicSpecs,
            ChronicChips.Where(c => c.IsSelected).Select(c => c.Id),
            ChronicOther);

    private async Task SaveAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var request = new MyPatientMedicalInfoUpdateRequest
            {
                BloodType = BloodType.Trim(),
                Allergies = ComposeAllergies(),
                ChronicConditions = ComposeChronic(),
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
