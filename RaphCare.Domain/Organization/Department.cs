namespace RaphCare.Domain.Organization;

/// <summary>
/// Internal clinic division (e.g. Cardiology, Pediatrics).
/// </summary>
/// <remarks>
/// Relationship: belongs to a specific <see cref="Clinic"/> via <c>ClinicId</c>.
/// Aggregate rationale: acts as a subdivision within the clinic domain boundary (not a standalone aggregate root).
/// </remarks>
public class Department : ClinicOwnedEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
