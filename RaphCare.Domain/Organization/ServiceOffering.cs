using RaphCare.Domain.Organization.Common;

namespace RaphCare.Domain.Organization;

/// <summary>
/// Service offered by a clinic or by a specific provider.
/// </summary>
///
/// <remarks>
/// Relationship: belongs to a <see cref="Clinic"/> via <c>ClinicId</c> and may be scoped to a particular <see cref="Provider"/> when <c>ProviderId</c> is set.
/// Aggregate rationale: supports both clinic-wide and provider-scoped catalog entries within the same domain model.
/// </remarks>
public class ServiceOffering : ClinicOwnedEntity
{
    public Guid? ProviderId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsTelemedicineAvailable { get; set; }
    public bool IsActive { get; set; }
    public Provider? Provider { get; set; }
}
