using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Organization;

/// <summary>
/// Top-level healthcare organization. Aggregate root for multi-clinic isolation.
/// </summary>
public class Clinic : AggregateRoot, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<Facility> Facilities { get; set; } = new List<Facility>();
    public ICollection<Department> Departments { get; set; } = new List<Department>();
    public ICollection<Provider> Providers { get; set; } = new List<Provider>();
    public ICollection<ServiceOffering> ServiceOfferings { get; set; } = new List<ServiceOffering>();
}
