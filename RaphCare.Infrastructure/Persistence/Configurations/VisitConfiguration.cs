using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Clinical;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> builder)
    {
        builder.ToTable("Visits");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.VisitType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Summary).HasMaxLength(4000);
        builder.HasIndex(e => e.ClinicId);
        builder.HasIndex(e => e.AppointmentId);
        builder.HasIndex(e => e.PatientId);
        builder.HasIndex(e => e.ProviderId);
        builder.HasMany(e => e.ClinicalNotes).WithOne(c => c.Visit).HasForeignKey(c => c.VisitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.Diagnoses).WithOne(d => d.Visit).HasForeignKey(d => d.VisitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.Prescriptions).WithOne(p => p.Visit).HasForeignKey(p => p.VisitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.SOAPNote).WithOne(s => s.Visit).HasForeignKey<SOAPNote>(s => s.VisitId).OnDelete(DeleteBehavior.Restrict);
    }
}
