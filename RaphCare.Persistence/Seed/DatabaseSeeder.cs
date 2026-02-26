using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RaphCare.Domain.Identity;
using RaphCare.Domain.Insurance;
using RaphCare.Domain.Organization;

namespace RaphCare.Persistence.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("RaphCare.Persistence.Seed.DatabaseSeeder");
        try
        {
            await using var scope = serviceProvider.CreateAsyncScope();

            await SeedRolesAndPermissionsAsync(scope.ServiceProvider, logger, cancellationToken).ConfigureAwait(false);
            await SeedClinicAsync(scope.ServiceProvider, logger, cancellationToken).ConfigureAwait(false);
            await SeedInsurancePlanAsync(scope.ServiceProvider, logger, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private static async Task SeedRolesAndPermissionsAsync(
        IServiceProvider scopedProvider,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var context = scopedProvider.GetService<IdentityDbContext>();
        if (context == null) return;

        if (await context.Roles.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            logger.LogInformation("Roles already seeded.");
            return;
        }

        var now = DateTime.UtcNow;
        var adminRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Administrator",
            Description = "Full system access",
            CreatedAt = now
        };
        var clinicianRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Clinician",
            Description = "Clinical access",
            CreatedAt = now
        };
        var patientRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Patient",
            Description = "Patient portal access",
            CreatedAt = now
        };

        context.Roles.AddRange(adminRole, clinicianRole, patientRole);

        var permissions = new[]
        {
            new Permission { Id = Guid.NewGuid(), Code = "Patients.Read", Name = "View patients", CreatedAt = now },
            new Permission { Id = Guid.NewGuid(), Code = "Patients.Write", Name = "Edit patients", CreatedAt = now },
            new Permission { Id = Guid.NewGuid(), Code = "Visits.Read", Name = "View visits", CreatedAt = now },
            new Permission { Id = Guid.NewGuid(), Code = "Visits.Write", Name = "Document visits", CreatedAt = now },
            new Permission { Id = Guid.NewGuid(), Code = "Admin.All", Name = "Administrative access", CreatedAt = now }
        };
        context.Permissions.AddRange(permissions);

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Roles and permissions seeded.");
    }

    private static async Task SeedClinicAsync(
        IServiceProvider scopedProvider,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var context = scopedProvider.GetService<ClinicalDbContext>();
        if (context == null) return;

        if (await context.Clinics.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            logger.LogInformation("Clinic already seeded.");
            return;
        }

        var clinic = new Clinic
        {
            Name = "RaphCare Demo Clinic",
            RegistrationNumber = "REG-DEMO-001",
            Country = "South Africa",
            TimeZone = "South Africa Standard Time",
            IsActive = true
        };
        context.Clinics.Add(clinic);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Example clinic seeded.");
    }

    private static async Task SeedInsurancePlanAsync(
        IServiceProvider scopedProvider,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var context = scopedProvider.GetService<InsuranceDbContext>();
        if (context == null) return;

        if (await context.InsurancePlans.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            logger.LogInformation("Insurance plans already seeded.");
            return;
        }

        var plan = new InsurancePlan
        {
            Name = "Standard Medical Aid",
            Code = "STD-001",
            Description = "Sample insurance plan for seeding",
            IsActive = true
        };
        context.InsurancePlans.Add(plan);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Sample insurance plan seeded.");
    }
}
