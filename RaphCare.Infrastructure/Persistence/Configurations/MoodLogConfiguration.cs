using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.MentalHealth;
using RaphCare.Domain.Patients;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public sealed class MoodLogConfiguration : IEntityTypeConfiguration<MoodLog>
{
    public void Configure(EntityTypeBuilder<MoodLog> builder)
    {
        builder.ToTable("MoodLogs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.MoodScore).IsRequired();
        builder.Property(e => e.LoggedAt).IsRequired();
        builder.Property(e => e.Notes).HasMaxLength(500);
        builder.HasIndex(e => e.PatientId);
        builder.HasIndex(e => new { e.PatientId, e.LoggedAt });
        builder.HasOne<Patient>()
            .WithMany()
            .HasForeignKey(e => e.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
