using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RaphCare.Infrastructure.Persistence.Interceptors;

namespace RaphCare.Persistence;

public static class DependencyInjection
{
    private const string DefaultConnectionName = "DefaultConnection";
    private const string MigrationsAssemblyName = "RaphCare.Persistence";

    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var env = configuration["ASPNETCORE_ENVIRONMENT"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";
        var isDevelopment = string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase);

        var connectionString = configuration.GetConnectionString(DefaultConnectionName)
            ?? throw new InvalidOperationException($"Connection string '{DefaultConnectionName}' not found.");

        string GetConnectionString(string name) =>
            configuration.GetConnectionString(name) ?? connectionString;

        services.AddDbContext<IdentityDbContext>((sp, options) =>
        {
            options.UseSqlServer(GetConnectionString("IdentityConnection"), sql =>
                sql.MigrationsAssembly(MigrationsAssemblyName));
            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<DomainEventDispatcherInterceptor>());
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            if (isDevelopment)
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

        services.AddDbContext<ClinicalDbContext>((sp, options) =>
        {
            options.UseSqlServer(GetConnectionString("ClinicalConnection"), sql =>
                sql.MigrationsAssembly(MigrationsAssemblyName));
            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<DomainEventDispatcherInterceptor>());
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            if (isDevelopment)
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

        services.AddDbContext<DeviceDbContext>((sp, options) =>
        {
            options.UseSqlServer(GetConnectionString("DeviceConnection"), sql =>
                sql.MigrationsAssembly(MigrationsAssemblyName));
            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<DomainEventDispatcherInterceptor>());
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            if (isDevelopment)
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

        services.AddDbContext<InsuranceDbContext>((sp, options) =>
        {
            options.UseSqlServer(GetConnectionString("InsuranceConnection"), sql =>
                sql.MigrationsAssembly(MigrationsAssemblyName));
            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<DomainEventDispatcherInterceptor>());
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            if (isDevelopment)
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

        services.AddDbContext<BillingDbContext>((sp, options) =>
        {
            options.UseSqlServer(GetConnectionString("BillingConnection"), sql =>
                sql.MigrationsAssembly(MigrationsAssemblyName));
            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<DomainEventDispatcherInterceptor>());
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            if (isDevelopment)
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

        services.AddDbContext<AIDbContext>((sp, options) =>
        {
            options.UseSqlServer(GetConnectionString("AIConnection"), sql =>
                sql.MigrationsAssembly(MigrationsAssemblyName));
            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<DomainEventDispatcherInterceptor>());
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            if (isDevelopment)
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

        return services;
    }
}
