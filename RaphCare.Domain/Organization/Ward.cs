using RaphCare.Domain.Organization.Common;

namespace RaphCare.Domain.Organization;

/// <summary>Hospital ward (unit) within a facility.</summary>
public class Ward : ClinicOwnedEntity
{
    public Guid FacilityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; } = true;

    public Facility Facility { get; set; } = null!;
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
