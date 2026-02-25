using RaphCare.Domain.Common;

namespace RaphCare.Domain.Organization;

/// <summary>
/// Internal clinic division (e.g. Cardiology, Pediatrics).
/// </summary>
public class Department : BaseEntity
{
    public Guid ClinicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Clinic Clinic { get; set; } = null!;
}
