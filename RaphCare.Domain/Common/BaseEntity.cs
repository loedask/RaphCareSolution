using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Common;

/// <summary>
/// Base class for all entities in the domain.
/// Provides identity, audit timestamps, and domain event collection support.
/// </summary>
public abstract class BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Call when the entity is updated so UpdatedAt can be set.
    /// Subclasses may expose this or wrap it in behavior.
    /// </summary>
    protected void SetUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
