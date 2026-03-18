using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Organization;

/// <summary>
/// Medical doctor or clinician.
/// </summary>
/// 
/// <remarks>
/// Aggregate rationale: aggregate root that keeps provider-related consistency for schedules and offerings.
/// Relationship: belongs to a specific <see cref="Clinic"/> via <c>ClinicId</c>.
/// </remarks>
public class Provider : ClinicOwnedAggregateRootEntity, ISoftDelete
{
    public Guid ApplicationUserId { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public ICollection<Specialty> Specialties { get; set; } = new List<Specialty>();
    public bool IsActive { get; set; }
    public DateTime? JoinedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public ICollection<ProviderSchedule> ProviderSchedules { get; set; } = new List<ProviderSchedule>();
    public ICollection<ServiceOffering> ServiceOfferings { get; set; } = new List<ServiceOffering>();
}
