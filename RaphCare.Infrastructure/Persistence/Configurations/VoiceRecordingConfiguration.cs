using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Patients;

namespace RaphCare.Infrastructure.Persistence.Configurations;

/// <summary>EF Core Fluent API configuration for voice onboarding recordings.</summary>
public class VoiceRecordingConfiguration : IEntityTypeConfiguration<VoiceRecording>
{
    public void Configure(EntityTypeBuilder<VoiceRecording> builder)
    {
        builder.ToTable("VoiceRecordings");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.StorageUrl).IsRequired().HasMaxLength(2048);
        builder.Property(e => e.Language).IsRequired().HasMaxLength(20);
        builder.Property(e => e.DurationSeconds).IsRequired();

        builder.HasOne(e => e.Patient)
            .WithMany(p => p.VoiceRecordings)
            .HasForeignKey(e => e.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.PatientId);
    }
}
