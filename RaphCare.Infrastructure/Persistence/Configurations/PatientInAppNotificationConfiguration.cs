using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Patients;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public sealed class PatientInAppNotificationConfiguration : IEntityTypeConfiguration<PatientInAppNotification>
{
    public void Configure(EntityTypeBuilder<PatientInAppNotification> builder)
    {
        builder.ToTable("PatientInAppNotifications");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Body).IsRequired().HasMaxLength(4000);
        builder.Property(e => e.Type).IsRequired().HasMaxLength(64);
        builder.HasIndex(e => e.PatientId);
        builder.HasIndex(e => new { e.PatientId, e.CreatedAt });
        builder.HasOne<Patient>()
            .WithMany()
            .HasForeignKey(e => e.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
