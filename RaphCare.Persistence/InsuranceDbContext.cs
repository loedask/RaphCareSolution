using Microsoft.EntityFrameworkCore;
using RaphCare.Domain.Insurance;
using RaphCare.Domain.Patients;
using RaphCare.Infrastructure.Persistence.Configurations;

namespace RaphCare.Persistence;

/// <summary>
/// Bounded context: Insurance plans and patient insurance profiles.
/// </summary>
public class InsuranceDbContext : DbContext
{
    public InsuranceDbContext(DbContextOptions<InsuranceDbContext> options) : base(options) { }

    public DbSet<InsurancePlan> InsurancePlans => Set<InsurancePlan>();
    public DbSet<InsuranceProfile> InsuranceProfiles => Set<InsuranceProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new InsurancePlanConfiguration());
        modelBuilder.ApplyConfiguration(new InsuranceProfileConfiguration());
        modelBuilder.ApplyPersistenceConventions();
    }
}
