using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RaphCare.Domain.Common.Interfaces;
using RaphCare.Domain.Patients;

namespace RaphCare.Persistence;

/// <summary>
/// Shared EF Core model conventions for all Persistence DbContexts:
/// global DeleteBehavior.Restrict, ISoftDelete query filters, patient-principal alignment, default string length.
/// </summary>
public static class DbContextModelBuilderExtensions
{
    public const int DefaultStringLength = 256;

    public static void ApplyPersistenceConventions(this ModelBuilder modelBuilder)
    {
        ApplyGlobalRestrictDeleteBehavior(modelBuilder);
        ApplySoftDeleteQueryFilters(modelBuilder);
        ApplyPatientPrincipalQueryFilters(modelBuilder);
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
    /// Aligns dependents with <see cref="Patient"/> soft-delete: when <see cref="Patient"/> has a query filter,
    /// required relationships to it otherwise trigger EF Core validation warning 10622. Dependents with a navigation
    /// to <see cref="Patient"/> get <c>!Patient.IsDeleted</c> (combined with any existing filter).
    /// </summary>
    private static void ApplyPatientPrincipalQueryFilters(ModelBuilder modelBuilder)
    {
        var patientEntity = modelBuilder.Model.FindEntityType(typeof(Patient));
        if (patientEntity?.GetQueryFilter() is null) return;

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.IsOwned()) continue;
            if (entityType.ClrType == typeof(Patient)) continue;

            var patientFks = entityType.GetForeignKeys()
                .Where(fk => fk.PrincipalEntityType.ClrType == typeof(Patient) && !fk.IsOwnership && fk.DependentToPrincipal is not null)
                .ToList();
            if (patientFks.Count == 0) continue;

            var clrType = entityType.ClrType;
            var parameter = Expression.Parameter(clrType, "e");

            Expression? patientAliveChain = null;
            foreach (var fk in patientFks)
            {
                var navigation = fk.DependentToPrincipal!;
                var navProperty = Expression.Property(parameter, navigation.Name);
                var isDeleted = Expression.Property(navProperty, nameof(Patient.IsDeleted));
                var notDeleted = Expression.Equal(isDeleted, Expression.Constant(false));
                patientAliveChain = patientAliveChain is null ? notDeleted : Expression.AndAlso(patientAliveChain, notDeleted);
            }

            var existingFilter = entityType.GetQueryFilter();
            Expression body = patientAliveChain!;
            if (existingFilter is not null)
            {
                var oldParam = existingFilter.Parameters[0];
                var visitor = new ReplaceParameterVisitor(oldParam, parameter);
                var existingBody = visitor.Visit(existingFilter.Body);
                body = Expression.AndAlso(existingBody!, patientAliveChain!);
            }

            modelBuilder.Entity(clrType).HasQueryFilter(Expression.Lambda(body, parameter));
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
