using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Patients;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public class PatientExternalIdConfiguration : IEntityTypeConfiguration<PatientExternalId>
{
    public void Configure(EntityTypeBuilder<PatientExternalId> builder)
    {
        builder.ToTable("PatientExternalIds");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.SourceSystem).IsRequired().HasMaxLength(256);
        builder.Property(e => e.ExternalId).IsRequired().HasMaxLength(256);

        builder.HasIndex(e => new { e.SourceSystem, e.ExternalId }).IsUnique();
    }
}

