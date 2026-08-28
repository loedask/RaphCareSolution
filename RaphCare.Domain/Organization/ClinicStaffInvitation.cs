using RaphCare.Domain.Common;

namespace RaphCare.Domain.Organization;

/// <summary>Email invitation for a professional who does not yet have a RaphCare account.</summary>
public class ClinicStaffInvitation : BaseEntity
{
    public Guid ClinicId { get; set; }
    public string Email { get; set; } = string.Empty;
    public Guid InvitedByApplicationUserId { get; set; }
    public DateTime InvitedAt { get; set; }
    public DateTime? LastInvitationSentAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public Guid? AcceptedApplicationUserId { get; set; }
    public bool IsCancelled { get; set; }
    public string JobRole { get; set; } = "Clinician";

    public Clinic Clinic { get; set; } = null!;
}
