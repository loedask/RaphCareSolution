using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.AI;

namespace RaphCare.Infrastructure.Persistence.Configurations;

/// <summary>EF Core Fluent API configuration for the WellnessInsight entity (AI bounded context).</summary>
public class WellnessInsightConfiguration : IEntityTypeConfiguration<WellnessInsight>
{
    public void Configure(EntityTypeBuilder<WellnessInsight> builder)
    {
        builder.ToTable("WellnessInsights");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.InsightType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(2000);
        builder.HasIndex(e => e.PatientId);
        builder.HasIndex(e => e.GeneratedAt);
    }
}
