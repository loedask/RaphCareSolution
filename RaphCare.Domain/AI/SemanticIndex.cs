using RaphCare.Domain.Common;

namespace RaphCare.Domain.AI;

public class SemanticIndex : BaseEntity
{
    public Guid RelatedEntityId { get; set; }

    public string IndexedText { get; set; } = null!;
    public DateTime IndexedAt { get; set; }
}

