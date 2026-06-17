using RaphCare.Domain.Organization.Shared;

namespace RaphCare.Domain.Organization;

/// <summary>
/// Clinic administrator with management responsibilities.
/// </summary>
/// 
/// <remarks>
/// Relationship: belongs to a single <see cref="Clinic"/> via <c>ClinicId</c> and is linked to the owning user via <c>ApplicationUserId</c>.
/// Aggregate rationale: this is not an aggregate root; its lifecycle should be managed within the enclosing domain boundary.
/// </remarks>
public class Administrator : ClinicOwnedEntity
{
    public Guid ApplicationUserId { get; set; }
    public string Title { get; set; } = string.Empty;
}
