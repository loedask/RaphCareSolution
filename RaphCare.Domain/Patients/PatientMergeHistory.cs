using RaphCare.Domain.Common;

namespace RaphCare.Domain.Patients;

/// <summary>
/// Audit record for MPI patient merge operations. Tracks which duplicate was merged into which primary and by whom.
/// </summary>
public class PatientMergeHistory : BaseEntity
{
    public Guid PrimaryPatientId { get; set; }
    public Guid MergedPatientId { get; set; }
    public DateTime MergedAt { get; set; }
    public Guid? MergedByUserId { get; set; }
}
