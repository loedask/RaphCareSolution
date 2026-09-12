using RaphCare.Domain.Common;

namespace RaphCare.Domain.Clinical;

/// <summary>
/// Patient agreement to a clinic's pre-visit consent for a specific appointment (title/body snapshotted at sign time).
/// </summary>
public class AppointmentConsent : BaseEntity
{
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ClinicId { get; set; }
    public Guid? TemplateId { get; set; }
    public string TitleSnapshot { get; set; } = string.Empty;
    public string BodySnapshot { get; set; } = string.Empty;
    public DateTime SignedAt { get; set; }
    public Guid SignedByApplicationUserId { get; set; }
}
