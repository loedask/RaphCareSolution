using RaphCare.Domain.Common;

namespace RaphCare.Domain.AI;

public class EmbeddingVector : BaseEntity
{
    public Guid RelatedEntityId { get; set; }

    public string EntityType { get; set; } = null!;
    public string VectorJson { get; set; } = null!;
}

