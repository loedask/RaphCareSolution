using RaphCare.Domain.Common;

namespace RaphCare.Domain.Patients;

/// <summary>
/// Immunization record for a patient.
/// </summary>
public class Immunization : BaseEntity
{
    public Guid PatientId { get; set; }
    public string VaccineName { get; set; } = string.Empty;
    public DateTime AdministeredOn { get; set; }
    public string? DoseNumber { get; set; }

    public Patient Patient { get; set; } = null!;
}
