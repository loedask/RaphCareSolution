using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Persistence;

/// <summary>
/// Shared EF Core model conventions for all Persistence DbContexts:
/// global DeleteBehavior.Restrict, ISoftDelete query filters, default string length.
/// </summary>
public static class DbContextModelBuilderExtensions
{
    public const int DefaultStringLength = 256;

    public static void ApplyPersistenceConventions(this ModelBuilder modelBuilder)
    {
        ApplyGlobalRestrictDeleteBehavior(modelBuilder);
        ApplySoftDeleteQueryFilters(modelBuilder);
        ApplyDefaultStringLength(modelBuilder);
    }

    private static void ApplyGlobalRestrictDeleteBehavior(ModelBuilder modelBuilder)
    {
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
    }

    private static void ApplySoftDeleteQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType)) continue;
            if (entityType.GetQueryFilter() != null) continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var property = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
            var condition = Expression.Equal(property, Expression.Constant(false));
            var lambda = Expression.Lambda(condition, parameter);
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }

    private static void ApplyDefaultStringLength(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(string) && property.GetMaxLength() == null)
                    property.SetMaxLength(DefaultStringLength);
            }
        }
    }
}
