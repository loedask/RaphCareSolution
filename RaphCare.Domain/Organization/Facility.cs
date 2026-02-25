using RaphCare.Domain.Common;

namespace RaphCare.Domain.Organization;

/// <summary>
/// Physical or virtual branch of a clinic.
/// </summary>
public class Facility : BaseEntity
{
    public Guid ClinicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsVirtual { get; set; }

    public Clinic Clinic { get; set; } = null!;
}
