using RaphCare.Domain.Patients.Common;

namespace RaphCare.Domain.Patients;

/// <summary>
/// Emergency contact for a patient.
/// </summary>
public class EmergencyContact : PatientOwnedEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Relationship { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}
