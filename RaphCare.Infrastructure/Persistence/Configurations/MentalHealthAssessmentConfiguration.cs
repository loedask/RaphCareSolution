using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.MentalHealth;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Patients;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public sealed class MentalHealthAssessmentConfiguration : IEntityTypeConfiguration<MentalHealthAssessment>
{
    public void Configure(EntityTypeBuilder<MentalHealthAssessment> builder)
    {
        builder.ToTable("MentalHealthAssessments");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.AssessmentType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.SeverityLevel).IsRequired().HasMaxLength(50);
        builder.Property(e => e.ConductedAt).IsRequired();
        builder.Property(e => e.TotalScore).HasPrecision(10, 2);
        builder.HasIndex(e => e.ClinicId);
        builder.HasIndex(e => e.PatientId);
        builder.HasIndex(e => new { e.ClinicId, e.ConductedAt });
        builder.HasOne<Clinic>()
            .WithMany()
            .HasForeignKey(e => e.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Patient>()
            .WithMany()
            .HasForeignKey(e => e.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.Questions)
            .WithOne(q => q.Assessment)
            .HasForeignKey(q => q.MentalHealthAssessmentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(e => e.Responses)
            .WithOne(r => r.Assessment)
            .HasForeignKey(r => r.MentalHealthAssessmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
