using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.AI;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public class RiskScoreConfiguration : IEntityTypeConfiguration<RiskScore>
{
    public void Configure(EntityTypeBuilder<RiskScore> builder)
    {
        builder.ToTable("RiskScores");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RiskCategory).IsRequired().HasMaxLength(100);
        builder.HasIndex(e => e.ClinicId);
        builder.HasIndex(e => e.PatientId);
        builder.HasIndex(e => e.CalculatedAt);
    }
}
