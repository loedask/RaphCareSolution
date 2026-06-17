using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;
using System.Reflection;

namespace RaphCare.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core SaveChanges interceptor. Uses the SavedChangesAsync lifecycle hook after a successful save to collect domain events from <see cref="Domain.Common.BaseEntity"/> entries, dispatch them via <see cref="IDomainEventDispatcher"/> (which publishes each as a MediatR notification), then clear the events from the entities.
/// Ensures domain events are published only after persistence succeeds.
/// </summary>
public class DomainEventDispatcherInterceptor(IDomainEventDispatcher dispatcher) : SaveChangesInterceptor
{
    private static readonly PropertyInfo? DomainEventsProp = typeof(BaseEntity)
        .GetProperty(nameof(BaseEntity.DomainEvents), BindingFlags.Public | BindingFlags.Instance);
    private static readonly MethodInfo? ClearMethod = typeof(BaseEntity).GetMethod(nameof(BaseEntity.ClearDomainEvents));

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(eventData.Context, cancellationToken).ConfigureAwait(false);
        return await base.SavedChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);
    }

    private async Task DispatchDomainEventsAsync(DbContext? context, CancellationToken cancellationToken)
    {
        if (context == null) return;

        var events = new List<IDomainEvent>();
        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (DomainEventsProp?.GetValue(entry.Entity) is IReadOnlyCollection<IDomainEvent> collection && collection.Count > 0)
            {
                events.AddRange(collection);
                ClearMethod?.Invoke(entry.Entity, null);
            }
        }

        if (events.Count > 0)
            await dispatcher.DispatchAsync(events, cancellationToken).ConfigureAwait(false);
    }
}
