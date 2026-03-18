using RaphCare.Domain.Common;
using RaphCare.Domain.Patients.Enums;

namespace RaphCare.Domain.Patients;

/// <summary>
/// Explicit patient->clinic access record used to enforce multi-tenant safety.
/// Access can be explicit (derived or manual) and later validated/extended.
/// </summary>
public class PatientClinicAccess : BaseEntity
{
    public Guid PatientId { get; set; }
    public Guid ClinicId { get; set; }

    public PatientClinicAccessType AccessType { get; set; }

    public DateTime GrantedAt { get; set; }
    public string GrantedByRule { get; set; } = string.Empty;

    public DateTime? LastValidatedAt { get; set; }

    public bool IsActive { get; set; }

    public string? Notes { get; set; }
}

