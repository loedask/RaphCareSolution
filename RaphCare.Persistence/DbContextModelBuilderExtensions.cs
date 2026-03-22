using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Persistence;

/// <summary>
/// Shared EF Core model conventions for all Persistence DbContexts:
/// global DeleteBehavior.Restrict, ISoftDelete query filters, soft-delete principal alignment, default string length.
/// </summary>
public static class DbContextModelBuilderExtensions
{
    public const int DefaultStringLength = 256;

    public static void ApplyPersistenceConventions(this ModelBuilder modelBuilder)
    {
        ApplyGlobalRestrictDeleteBehavior(modelBuilder);
        ApplySoftDeleteQueryFilters(modelBuilder);
        ApplySoftDeletePrincipalQueryFilters(modelBuilder);
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

    /// <summary>
    /// Aligns dependents with any <see cref="ISoftDelete"/> principal that has a global query filter (Patient, Clinic,
    /// Provider, etc.). Otherwise EF Core validation warning 10622 applies. Optional foreign keys use
    /// <c>FK == null || !Principal.IsDeleted</c>.
    /// </summary>
    private static void ApplySoftDeletePrincipalQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.IsOwned()) continue;

            var principalFks = entityType.GetForeignKeys()
                .Where(fk =>
                    !fk.IsOwnership &&
                    fk.DependentToPrincipal is not null &&
                    typeof(ISoftDelete).IsAssignableFrom(fk.PrincipalEntityType.ClrType) &&
                    fk.PrincipalEntityType.GetQueryFilter() is not null)
                .ToList();
            if (principalFks.Count == 0) continue;

            var clrType = entityType.ClrType;
            var parameter = Expression.Parameter(clrType, "e");

            Expression? principalAliveChain = null;
            foreach (var fk in principalFks)
            {
                var expr = BuildPrincipalNotSoftDeleted(parameter, fk);
                principalAliveChain = principalAliveChain is null ? expr : Expression.AndAlso(principalAliveChain, expr);
            }

            var existingFilter = entityType.GetQueryFilter();
            Expression body = principalAliveChain!;
            if (existingFilter is not null)
            {
                var oldParam = existingFilter.Parameters[0];
                var visitor = new ReplaceParameterVisitor(oldParam, parameter);
                var existingBody = visitor.Visit(existingFilter.Body);
                body = Expression.AndAlso(existingBody!, principalAliveChain!);
            }

            modelBuilder.Entity(clrType).HasQueryFilter(Expression.Lambda(body, parameter));
        }
    }

    private static Expression BuildPrincipalNotSoftDeleted(ParameterExpression parameter, IMutableForeignKey fk)
    {
        var navigation = fk.DependentToPrincipal!;
        var navProperty = Expression.Property(parameter, navigation.Name);
        var isDeleted = Expression.Property(navProperty, nameof(ISoftDelete.IsDeleted));
        var notDeleted = Expression.Equal(isDeleted, Expression.Constant(false));

        if (fk.IsRequired)
            return notDeleted;

        var fkProperty = fk.Properties[0];
        var fkValue = Expression.Property(parameter, fkProperty.Name);
        var nullConst = Expression.Constant(null, fkProperty.ClrType);
        var fkIsNull = Expression.Equal(fkValue, nullConst);
        return Expression.OrElse(fkIsNull, notDeleted);
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

    private sealed class ReplaceParameterVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _old;
        private readonly ParameterExpression _new;

        public ReplaceParameterVisitor(ParameterExpression oldParam, ParameterExpression newParam)
        {
            _old = oldParam;
            _new = newParam;
        }

        protected override Expression VisitParameter(ParameterExpression node) => node == _old ? _new : node;
    }
}
