using RaphCare.Domain.Organization.Shared;

namespace RaphCare.Domain.Organization;

/// <summary>
/// Physical or virtual branch of a clinic.
/// </summary>
///
/// <remarks>
/// Relationship: belongs to a specific <see cref="Clinic"/> via <c>ClinicId</c>.
/// Aggregate rationale: a clinic-scoped entity (not an aggregate root).
/// </remarks>
public class Facility : ClinicOwnedEntity
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsVirtual { get; set; }
}
