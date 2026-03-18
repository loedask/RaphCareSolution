using RaphCare.Domain.Common;
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
public class Provider : AggregateRoot, ISoftDelete
{
    public Guid ClinicId { get; set; }
    public Guid ApplicationUserId { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? JoinedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Clinic Clinic { get; set; } = null!;
    public ICollection<ProviderSchedule> ProviderSchedules { get; set; } = new List<ProviderSchedule>();
    public ICollection<ServiceOffering> ServiceOfferings { get; set; } = new List<ServiceOffering>();
}
