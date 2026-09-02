using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Clinical;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public sealed class InpatientAdmissionConfiguration : IEntityTypeConfiguration<InpatientAdmission>
{
    public void Configure(EntityTypeBuilder<InpatientAdmission> builder)
    {
        builder.ToTable("InpatientAdmissions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Reason).HasMaxLength(500);
        builder.Property(e => e.Notes).HasMaxLength(1000);
        builder.Property(e => e.DischargeSummary).HasMaxLength(4000);
        builder.Property(e => e.AdmittedAt).IsRequired();
        builder.HasIndex(e => e.ClinicId);
        builder.HasIndex(e => e.PatientId);
        builder.HasIndex(e => e.BedId);
        builder.HasIndex(e => new { e.ClinicId, e.Status });
        builder.HasOne(e => e.Clinic)
            .WithMany()
            .HasForeignKey(e => e.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Patient)
            .WithMany()
            .HasForeignKey(e => e.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Bed)
            .WithMany()
            .HasForeignKey(e => e.BedId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
