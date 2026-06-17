namespace RaphCare.Domain.Common.Interfaces;

/// <summary>
/// Represents a domain event that occurred within an aggregate.
/// Domain events are immutable facts about something that happened.
/// </summary>
public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}
