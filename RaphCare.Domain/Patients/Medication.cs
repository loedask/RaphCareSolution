using RaphCare.Domain.Common;

namespace RaphCare.Domain.Patients;

/// <summary>
/// Current or past medication for a patient.
/// </summary>
public class Medication : BaseEntity
{
    public Guid PatientId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public Patient Patient { get; set; } = null!;
}
