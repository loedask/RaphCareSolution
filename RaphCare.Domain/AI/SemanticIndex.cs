using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.AI;

public class SemanticIndex : BaseEntity
{
    public Guid RelatedEntityId { get; set; }

    public string IndexedText { get; set; } = null!;
    public DateTime IndexedAt { get; set; }
}

