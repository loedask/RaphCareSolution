using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Patients;

namespace RaphCare.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for PatientClinicAccess.</summary>
public class PatientClinicAccessConfiguration : IEntityTypeConfiguration<PatientClinicAccess>
{
    public void Configure(EntityTypeBuilder<PatientClinicAccess> builder)
    {
        builder.ToTable("PatientClinicAccess");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.PatientId).IsRequired();
        builder.Property(e => e.ClinicId).IsRequired();
        builder.Property(e => e.AccessType).IsRequired();
        builder.Property(e => e.GrantedAt).IsRequired();
        builder.Property(e => e.GrantedByRule).IsRequired().HasMaxLength(256);
        builder.Property(e => e.LastValidatedAt);
        builder.Property(e => e.IsActive).IsRequired();
        builder.Property(e => e.Notes).HasMaxLength(2000);

        builder.HasIndex(e => new { e.PatientId, e.ClinicId }).IsUnique();
        builder.HasIndex(e => e.ClinicId);
        builder.HasIndex(e => e.IsActive);

        builder.HasOne<Patient>()
            .WithMany()
            .HasForeignKey(e => e.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

