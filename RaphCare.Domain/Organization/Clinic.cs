using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Organization;

/// <summary>
/// Top-level healthcare organization.
/// </summary>
/// 
/// <remarks>
/// Aggregate rationale: this is the aggregate root for multi-clinic isolation.
/// Relationship: owns <c>Facilities</c>, <c>Departments</c>, <c>Providers</c>, and <c>ServiceOfferings</c> collections.
/// </remarks>
public class Clinic : AggregateRoot, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? RegisteredByApplicationUserId { get; set; }

    public ICollection<Facility> Facilities { get; set; } = new List<Facility>();
    public ICollection<Department> Departments { get; set; } = new List<Department>();
    public ICollection<Provider> Providers { get; set; } = new List<Provider>();
    public ICollection<ServiceOffering> ServiceOfferings { get; set; } = new List<ServiceOffering>();
    public ICollection<ClinicStaffMembership> StaffMemberships { get; set; } = new List<ClinicStaffMembership>();
}
