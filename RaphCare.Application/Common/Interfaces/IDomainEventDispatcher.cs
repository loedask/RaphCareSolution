using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Application.Common.Interfaces;

/// <summary>
/// Dispatches domain events after persistence (e.g. via MediatR).
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>Dispatches collected domain events (e.g. by publishing as MediatR notifications after SaveChanges).</summary>
    /// <param name="domainEvents">The events raised by entities during the current unit of work.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
