using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Reflection;

namespace RaphCare.Infrastructure.Persistence.Interceptors;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private static readonly PropertyInfo? CreatedAtProp = typeof(Domain.Common.BaseEntity)
        .GetProperty(nameof(Domain.Common.BaseEntity.CreatedAt));
    private static readonly PropertyInfo? UpdatedAtProp = typeof(Domain.Common.BaseEntity)
        .GetProperty(nameof(Domain.Common.BaseEntity.UpdatedAt));

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        SetAuditProperties(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        SetAuditProperties(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void SetAuditProperties(DbContext? context)
    {
        if (context == null) return;

        var now = DateTime.UtcNow;
        foreach (var entry in context.ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                SetProperty(entry.Entity, CreatedAtProp, now);
                SetProperty(entry.Entity, UpdatedAtProp, now);
            }
            else if (entry.State == EntityState.Modified)
            {
                SetProperty(entry.Entity, UpdatedAtProp, now);
            }
        }
    }

    private static void SetProperty(object entity, PropertyInfo? property, DateTime value)
    {
        if (property == null) return;
        var setter = property.GetSetMethod(nonPublic: true);
        setter?.Invoke(entity, new object[] { value });
    }
}
