using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Clinical;

namespace RaphCare.Infrastructure.Persistence.Configurations;

/// <summary>EF Core Fluent API configuration for vital sign measurements.</summary>
public class VitalSignRecordConfiguration : IEntityTypeConfiguration<VitalSignRecord>
{
    public void Configure(EntityTypeBuilder<VitalSignRecord> builder)
    {
        builder.ToTable("VitalSignRecord");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Type).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Value).HasPrecision(18, 4);
        builder.Property(e => e.Unit).HasMaxLength(32);
        builder.HasIndex(e => e.VisitId);
    }
}
