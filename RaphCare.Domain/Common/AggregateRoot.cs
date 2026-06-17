using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Common;

/// <summary>
/// Base class for aggregate roots.
/// Aggregates are the entry points for changes and the consistency boundary for domain events.
/// </summary>
public abstract class AggregateRoot : BaseEntity, IAggregateRoot
{
    protected AggregateRoot()
    {
    }
}
