using RaphCare.Domain.Common;

namespace RaphCare.Domain.Organization;

/// <summary>Links an identity user to a clinic they may operate in the admin portal.</summary>
public class ClinicStaffMembership : BaseEntity
{
    public Guid ApplicationUserId { get; set; }
    public Guid ClinicId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime JoinedAt { get; set; }
    public DateTime? LastInvitationSentAt { get; set; }

    public Clinic Clinic { get; set; } = null!;
}
