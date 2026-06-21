namespace RaphCare.Client.Models.MedicalInfo;

public sealed class MyPatientMedicalInfoViewModel
{
    public string BloodType { get; set; } = string.Empty;
    public string Allergies { get; set; } = string.Empty;
    public string ChronicConditions { get; set; } = string.Empty;
    public string Medications { get; set; } = string.Empty;
    public string PrimaryDoctor { get; set; } = string.Empty;
}

public sealed class MyPatientMedicalInfoUpdateRequest
{
    public string BloodType { get; set; } = string.Empty;
    public string Allergies { get; set; } = string.Empty;
    public string ChronicConditions { get; set; } = string.Empty;
    public string Medications { get; set; } = string.Empty;
    public string PrimaryDoctor { get; set; } = string.Empty;
}
