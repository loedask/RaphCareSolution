using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Persistence;

/// <summary>
/// Shared EF Core model conventions for all Persistence DbContexts:
/// global DeleteBehavior.Restrict, ISoftDelete query filters, principal query-filter alignment, default string length.
/// </summary>
public static class DbContextModelBuilderExtensions
{
    public const int DefaultStringLength = 256;

    public static void ApplyPersistenceConventions(this ModelBuilder modelBuilder)
    {
        ApplyGlobalRestrictDeleteBehavior(modelBuilder);
        ApplySoftDeleteQueryFilters(modelBuilder);
        ApplyPrincipalQueryFilterAlignment(modelBuilder);
        ApplyDefaultStringLength(modelBuilder);
    }

    /// <summary>
    /// Marks every mapped table except <paramref name="ownedEntityTypes"/> as excluded from migrations.
    /// Use when multiple DbContexts share one database: the owning context migrates shared tables.
    /// </summary>
    public static void ExcludeNonOwnedTablesFromMigrations(this ModelBuilder modelBuilder, params Type[] ownedEntityTypes)
    {
        var owned = new HashSet<Type>(ownedEntityTypes);
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (owned.Contains(entityType.ClrType))
                continue;
            entityType.SetIsTableExcludedFromMigrations(true);
        }
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
    /// Two-pass alignment so EF Core warning 10622 is satisfied: (1) <see cref="ISoftDelete"/> principals; (2) any other
    /// principal that already has a global filter (e.g. ProviderSchedule after pass 1).
    /// Pass 2 rewrites the principal's filter by substituting its parameter with the dependent→principal navigation.
    /// Optional foreign keys use <c>FK == null || (principal visibility)</c>.
    /// </summary>
    private static void ApplyPrincipalQueryFilterAlignment(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.IsOwned()) continue;
            ApplyPrincipalQueryFilterAlignmentForEntity(modelBuilder, entityType, softDeletePrincipalsOnly: true);
        }

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.IsOwned()) continue;
            ApplyPrincipalQueryFilterAlignmentForEntity(modelBuilder, entityType, softDeletePrincipalsOnly: false);
        }
    }

    private static void ApplyPrincipalQueryFilterAlignmentForEntity(
        ModelBuilder modelBuilder,
        IMutableEntityType entityType,
        bool softDeletePrincipalsOnly)
    {
        var principalFks = entityType.GetForeignKeys()
            .Where(fk =>
            {
                if (fk.IsOwnership || fk.DependentToPrincipal is null) return false;
                if (fk.PrincipalEntityType.GetQueryFilter() is null) return false;
                var principalClr = fk.PrincipalEntityType.ClrType;
                var isSoftPrincipal = typeof(ISoftDelete).IsAssignableFrom(principalClr);
                return softDeletePrincipalsOnly ? isSoftPrincipal : !isSoftPrincipal;
            })
            .ToList();
        if (principalFks.Count == 0) return;

        var clrType = entityType.ClrType;
        var parameter = Expression.Parameter(clrType, "e");

        Expression? principalAliveChain = null;
        foreach (var fk in principalFks)
        {
            var principalFilter = fk.PrincipalEntityType.GetQueryFilter();
            if (principalFilter is null) continue;
            if (!softDeletePrincipalsOnly && principalFilter.Parameters.Count != 1)
                continue;

            Expression expr = softDeletePrincipalsOnly
                ? BuildPrincipalNotSoftDeleted(parameter, fk)
                : ComposePrincipalFilterThroughNavigation(parameter, fk, principalFilter);
            principalAliveChain = principalAliveChain is null ? expr : Expression.AndAlso(principalAliveChain, expr);
        }

        if (principalAliveChain is null) return;

        var existingFilter = entityType.GetQueryFilter();
        Expression body = principalAliveChain;
        if (existingFilter is not null)
        {
            var oldParam = existingFilter.Parameters[0];
            var visitor = new ReplaceParameterVisitor(oldParam, parameter);
            var existingBody = visitor.Visit(existingFilter.Body);
            body = Expression.AndAlso(existingBody!, principalAliveChain);
        }

        modelBuilder.Entity(clrType).HasQueryFilter(Expression.Lambda(body, parameter));
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

    private static Expression ComposePrincipalFilterThroughNavigation(
        ParameterExpression dependentParameter,
        IMutableForeignKey fk,
        LambdaExpression principalFilter)
    {
        var navigation = fk.DependentToPrincipal!;
        var navigatedPrincipal = Expression.Property(dependentParameter, navigation.Name);
        var principalParam = principalFilter.Parameters[0];
        var visitor = new ReplaceParameterWithExpressionVisitor(principalParam, navigatedPrincipal);
        var rewritten = visitor.Visit(principalFilter.Body);

        if (fk.IsRequired)
            return rewritten!;

        var fkProperty = fk.Properties[0];
        var fkValue = Expression.Property(dependentParameter, fkProperty.Name);
        var nullConst = Expression.Constant(null, fkProperty.ClrType);
        var fkIsNull = Expression.Equal(fkValue, nullConst);
        return Expression.OrElse(fkIsNull, rewritten!);
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

    private sealed class ReplaceParameterVisitor(ParameterExpression oldParam, ParameterExpression newParam) : ExpressionVisitor
    {
        private readonly ParameterExpression _old = oldParam;
        private readonly ParameterExpression _new = newParam;

        protected override Expression VisitParameter(ParameterExpression node) => node == _old ? _new : node;
    }

    private sealed class ReplaceParameterWithExpressionVisitor(ParameterExpression param, Expression replacement) : ExpressionVisitor
    {
        private readonly ParameterExpression _param = param;
        private readonly Expression _replacement = replacement;

        protected override Expression VisitParameter(ParameterExpression node) => node == _param ? _replacement : node;
    }
}
