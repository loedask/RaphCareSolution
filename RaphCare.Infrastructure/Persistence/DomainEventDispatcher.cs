using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Infrastructure.Persistence;

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
