using Microsoft.EntityFrameworkCore;
using RaphCare.Domain.AI;
using RaphCare.Infrastructure.Persistence.Configurations;

namespace RaphCare.Persistence;

/// <summary>
/// Bounded context: AI-generated insights and risk scores. Optimized for read-heavy workloads.
/// </summary>
public class AIDbContext : DbContext
{
    public AIDbContext(DbContextOptions<AIDbContext> options) : base(options) { }

    public DbSet<WellnessInsight> WellnessInsights => Set<WellnessInsight>();
    public DbSet<RiskScore> RiskScores => Set<RiskScore>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new WellnessInsightConfiguration());
        modelBuilder.ApplyConfiguration(new RiskScoreConfiguration());
        modelBuilder.ApplyPersistenceConventions();
    }
}
