using RaphCare.Domain.Common;

namespace RaphCare.Domain.Compliance;

/// <summary>
/// Record of consent granted or revoked for a patient (e.g. POPIA, HIPAA).
/// </summary>
public class ConsentGrant : BaseEntity
{
    public Guid PatientId { get; set; }
    public string ConsentType { get; set; } = string.Empty;
    public bool Granted { get; set; }
    public DateTime GrantedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}
