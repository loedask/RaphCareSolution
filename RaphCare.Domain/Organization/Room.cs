using RaphCare.Domain.Common;

namespace RaphCare.Domain.Organization;

/// <summary>Room within a ward.</summary>
public class Room : BaseEntity
{
    public Guid WardId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? RoomType { get; set; }
    public bool IsActive { get; set; } = true;

    public Ward Ward { get; set; } = null!;
    public ICollection<Bed> Beds { get; set; } = new List<Bed>();
}
