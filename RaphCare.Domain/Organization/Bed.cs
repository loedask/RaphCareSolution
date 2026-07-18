using RaphCare.Domain.Common;

namespace RaphCare.Domain.Organization;

/// <summary>Bed within a room. Status: Available / Occupied / Maintenance.</summary>
public class Bed : BaseEntity
{
    public Guid RoomId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Status { get; set; } = "Available";
    public bool IsActive { get; set; } = true;

    public Room Room { get; set; } = null!;
}
