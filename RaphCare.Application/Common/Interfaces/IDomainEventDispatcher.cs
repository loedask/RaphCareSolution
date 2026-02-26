using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Application.Common.Interfaces;

/// <summary>
/// Dispatches domain events after persistence (e.g. via MediatR).
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
