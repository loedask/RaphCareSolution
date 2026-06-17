using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Common;

/// <summary>
/// Base class for all domain events.
/// Domain events are immutable and capture something that happened in the domain.
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }

    protected DomainEvent()
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }
}
