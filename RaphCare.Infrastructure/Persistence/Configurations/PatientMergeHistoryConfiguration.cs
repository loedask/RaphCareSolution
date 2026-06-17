using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Patients;

namespace RaphCare.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for PatientMergeHistory audit records.</summary>
public class PatientMergeHistoryConfiguration : IEntityTypeConfiguration<PatientMergeHistory>
{
    public void Configure(EntityTypeBuilder<PatientMergeHistory> builder)
    {
        builder.ToTable("PatientMergeHistory");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.PrimaryPatientId).IsRequired();
        builder.Property(e => e.MergedPatientId).IsRequired();
        builder.Property(e => e.MergedAt).IsRequired();
        builder.Property(e => e.MergedByUserId);
        builder.HasIndex(e => e.PrimaryPatientId);
        builder.HasIndex(e => e.MergedPatientId);
        builder.HasIndex(e => e.MergedAt);
    }
}
