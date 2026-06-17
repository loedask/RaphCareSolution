using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core SaveChanges interceptor. Uses the SavingChanges / SavingChangesAsync lifecycle hook to convert physical deletes into soft deletes for entities implementing <see cref="RaphCare.Domain.Common.Interfaces.ISoftDelete"/> (sets IsDeleted and DeletedAt, then marks as Modified).
/// Exists to enforce soft-delete behavior consistently across all DbContexts.
/// </summary>
public class SoftDeleteInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ConvertDeletedToSoftDelete(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ConvertDeletedToSoftDelete(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void ConvertDeletedToSoftDelete(DbContext? context)
    {
        if (context == null) return;

        var now = DateTime.UtcNow;
        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Deleted) continue;
            if (entry.Entity is not ISoftDelete softDelete) continue;

            entry.State = EntityState.Modified;
            softDelete.IsDeleted = true;
            softDelete.DeletedAt = now;
        }
    }
}
