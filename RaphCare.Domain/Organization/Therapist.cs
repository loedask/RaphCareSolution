using RaphCare.Domain.Common;

namespace RaphCare.Domain.Organization;

/// <summary>
/// Mental-health provider (e.g. psychologist, counsellor).
/// </summary>
/// 
/// <remarks>
/// Relationship: belongs to a specific <see cref="Clinic"/> via <c>ClinicId</c>.
/// Aggregate rationale: clinic-scoped entity (not an aggregate root).
/// </remarks>
public class Therapist : BaseEntity
{
    public Guid ClinicId { get; set; }
    public Guid ApplicationUserId { get; set; }
    public string Certification { get; set; } = string.Empty;
    public ICollection<Specialty> Specialties { get; set; } = new List<Specialty>();
    public bool IsActive { get; set; }

    public Clinic Clinic { get; set; } = null!;
}
