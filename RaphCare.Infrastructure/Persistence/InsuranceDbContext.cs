using Microsoft.EntityFrameworkCore;
using RaphCare.Domain.Insurance;
using RaphCare.Domain.Patients;

namespace RaphCare.Infrastructure.Persistence;

public class InsuranceDbContext(DbContextOptions<InsuranceDbContext> options) : DbContext(options)
{
    public DbSet<InsurancePlan> InsurancePlans => Set<InsurancePlan>();
    public DbSet<InsuranceProfile> InsuranceProfiles => Set<InsuranceProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new Configurations.InsurancePlanConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.InsuranceProfileConfiguration());
    }
}
