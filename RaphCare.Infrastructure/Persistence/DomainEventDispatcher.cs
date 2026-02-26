using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Infrastructure.Persistence;

/// <summary>
/// Dispatches domain events by wrapping each <see cref="IDomainEvent"/> in a MediatR notification and publishing it, so handlers (e.g. in Application or API) can react to domain events after SaveChanges.
/// </summary>
public class DomainEventDispatcher(IMediator mediator) : IDomainEventDispatcher
{
    public async Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            await mediator.Publish(new DomainEventNotification(domainEvent), cancellationToken).ConfigureAwait(false);
        }
    }
}

internal sealed class DomainEventNotification(IDomainEvent domainEvent) : INotification
{
    public IDomainEvent DomainEvent { get; } = domainEvent;
}
