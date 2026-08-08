using Microsoft.EntityFrameworkCore;
using RaphCare.Persistence;
using RaphCare.Persistence.Seed;

namespace RaphCare.API.App.Extensions;

public static class DatabaseMigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;

        var env = services.GetRequiredService<IWebHostEnvironment>();

        // Local Development and Azure Staging / test hosts. Production stays manual / CI migrate.
        if (!env.IsDevelopment() && !env.IsStaging())
            return;

        var contexts = new DbContext[]
        {
            services.GetRequiredService<ClinicalDbContext>(),
            services.GetRequiredService<DeviceDbContext>(),
            services.GetRequiredService<InsuranceDbContext>(),
            services.GetRequiredService<BillingDbContext>(),
            services.GetRequiredService<AIDbContext>(),
            services.GetRequiredService<IdentityDbContext>()
        };

        foreach (var context in contexts)
        {
            await context.Database.MigrateAsync();
        }

        await DatabaseSeeder.SeedAsync(services);
    }
}
