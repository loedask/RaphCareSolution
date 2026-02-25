using RaphCare.Domain.Common;

namespace RaphCare.Domain.Organization;

/// <summary>
/// Mental-health provider (e.g. psychologist, counsellor).
/// </summary>
public class Therapist : BaseEntity
{
    public Guid ClinicId { get; set; }
    public Guid ApplicationUserId { get; set; }
    public string Certification { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public Clinic Clinic { get; set; } = null!;
}
