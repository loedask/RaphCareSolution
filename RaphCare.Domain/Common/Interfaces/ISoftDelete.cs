namespace RaphCare.Domain.Common.Interfaces;

/// <summary>
/// Enables soft-delete behavior for entities.
/// When implemented, the entity is logically deleted rather than physically removed.
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
}
