using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Reporting;

namespace RaphCare.Infrastructure.Persistence.Configurations;

/// <summary>EF Core Fluent API configuration for the DashboardSnapshot entity (reporting / AI context).</summary>
public class DashboardSnapshotConfiguration : IEntityTypeConfiguration<DashboardSnapshot>
{
    public void Configure(EntityTypeBuilder<DashboardSnapshot> builder)
    {
        builder.ToTable("DashboardSnapshots");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.SnapshotJson).IsRequired();
        builder.HasIndex(e => e.ClinicId);
        builder.HasIndex(e => e.SnapshotDate);
        builder.HasIndex(e => new { e.ClinicId, e.SnapshotDate }).IsUnique();
    }
}
