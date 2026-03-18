namespace RaphCare.Domain.Organization;

/// <summary>
/// Non-clinical support staff (reception, billing, etc.).
/// </summary>
/// 
/// <remarks>
/// Relationship: belongs to a specific <see cref="Clinic"/> via <c>ClinicId</c>.
/// Aggregate rationale: clinic-scoped entity (not an aggregate root) representing staff identity via <c>ApplicationUserId</c> and role via <c>RoleName</c>.
/// </remarks>
public class SupportStaff : ClinicOwnedEntity
{
    public Guid ApplicationUserId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}
