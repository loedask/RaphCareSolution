using RaphCare.Domain.Common;

namespace RaphCare.Domain.Organization;

/// <summary>
/// Non-clinical support staff (reception, billing, etc.).
/// </summary>
public class SupportStaff : BaseEntity
{
    public Guid ClinicId { get; set; }
    public Guid ApplicationUserId { get; set; }
    public string RoleName { get; set; } = string.Empty;

    public Clinic Clinic { get; set; } = null!;
}
