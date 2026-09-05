namespace RaphCare.Application.Features.PatientClinics.DTOs;

public sealed class PatientLinkedClinicDto
{
    public Guid ClinicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ReferenceCode { get; set; } = string.Empty;

    /// <summary>Access kind for display: Registered, ManualGrant, EncounterBased, InsuranceLinked, or CareHistory.</summary>
    public string AccessKind { get; set; } = string.Empty;

    public DateTime? GrantedAt { get; set; }
}
