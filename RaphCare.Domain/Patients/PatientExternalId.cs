using RaphCare.Domain.Common;

namespace RaphCare.Domain.Patients;

/// <summary>
/// External system identifier for a global patient record (e.g., clinic, insurer, lab, national registry).
/// </summary>
public class PatientExternalId : BaseEntity
{
    public Guid PatientId { get; set; }

    /// <summary>
    /// Logical name of the external source system (e.g., \"ClinicA\", \"InsuranceProvider\", \"NationalRegistry\").
    /// </summary>
    public string SourceSystem { get; set; } = string.Empty;

    /// <summary>
    /// The external system's identifier for this patient.
    /// </summary>
    public string ExternalId { get; set; } = string.Empty;
}

