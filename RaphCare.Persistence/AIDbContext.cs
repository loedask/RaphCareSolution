using Microsoft.EntityFrameworkCore;
using RaphCare.Domain.AI;
using RaphCare.Domain.Reporting;
using RaphCare.Infrastructure.Persistence.Configurations;

namespace RaphCare.Persistence;

/// <summary>
/// Bounded context: AI-generated insights, risk scores, and reporting snapshots. Optimized for read-heavy workloads.
/// </summary>
public class AIDbContext(DbContextOptions<AIDbContext> options) : DbContext(options)
{
    public DbSet<WellnessInsight> WellnessInsights => Set<WellnessInsight>();
    public DbSet<RiskScore> RiskScores => Set<RiskScore>();
    public DbSet<DashboardSnapshot> DashboardSnapshots => Set<DashboardSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new WellnessInsightConfiguration());
        modelBuilder.ApplyConfiguration(new RiskScoreConfiguration());
        modelBuilder.ApplyConfiguration(new DashboardSnapshotConfiguration());
        modelBuilder.ApplyPersistenceConventions();
    }
}
