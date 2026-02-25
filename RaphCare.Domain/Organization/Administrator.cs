using RaphCare.Domain.Common;

namespace RaphCare.Domain.Organization;

/// <summary>
/// Clinic administrator with management responsibilities.
/// </summary>
public class Administrator : BaseEntity
{
    public Guid ClinicId { get; set; }
    public Guid ApplicationUserId { get; set; }
    public string Title { get; set; } = string.Empty;

    public Clinic Clinic { get; set; } = null!;
}
