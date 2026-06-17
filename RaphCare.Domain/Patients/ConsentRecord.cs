using RaphCare.Domain.Common;

namespace RaphCare.Domain.Patients;

/// <summary>
/// Legal consent record (e.g. POPIA, HIPAA).
/// </summary>
public class ConsentRecord : BaseEntity
{
    public Guid PatientId { get; set; }
    public string ConsentType { get; set; } = string.Empty;
    public bool Granted { get; set; }
    public DateTime GrantedAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public Patient Patient { get; set; } = null!;
}
