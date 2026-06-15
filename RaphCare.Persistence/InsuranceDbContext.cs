using Microsoft.EntityFrameworkCore;
using RaphCare.Domain.Insurance;
using RaphCare.Domain.Patients;
using RaphCare.Infrastructure.Persistence.Configurations;

namespace RaphCare.Persistence;

/// <summary>
/// Bounded context: Insurance plans and patient insurance profiles.
/// </summary>
public class InsuranceDbContext(DbContextOptions<InsuranceDbContext> options) : DbContext(options)
{
    public DbSet<InsurancePlan> InsurancePlans => Set<InsurancePlan>();
    public DbSet<InsuranceProfile> InsuranceProfiles => Set<InsuranceProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new InsurancePlanConfiguration());
        modelBuilder.ApplyConfiguration(new InsuranceProfileConfiguration());

        // ClinicalDbContext owns Patients and the patient aggregate when contexts share one database.
        modelBuilder.Entity<Patient>(b =>
        {
            b.ToTable("Patients", t => t.ExcludeFromMigrations());
            b.HasKey(e => e.Id);
        });

        modelBuilder.ApplyPersistenceConventions();
        modelBuilder.ExcludeNonOwnedTablesFromMigrations(typeof(InsurancePlan), typeof(InsuranceProfile));
    }
}
