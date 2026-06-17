using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Devices;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public class DeviceEmergencyEventConfiguration : IEntityTypeConfiguration<DeviceEmergencyEvent>
{
    public void Configure(EntityTypeBuilder<DeviceEmergencyEvent> builder)
    {
        builder.ToTable("DeviceEmergencyEvents");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EventType).IsRequired().HasMaxLength(64);
        builder.Property(e => e.ExternalCorrelationId).HasMaxLength(256);
        builder.Property(e => e.CaregiverNotificationSummary).HasMaxLength(512);
        builder.HasIndex(e => e.DeviceId);
        builder.HasIndex(e => e.PatientId);
        builder.HasIndex(e => e.ClinicId);
        builder.HasIndex(e => e.OccurredAtUtc);
        builder.HasIndex(e => new { e.DeviceId, e.ExternalCorrelationId })
            .IsUnique()
            .HasFilter("[ExternalCorrelationId] IS NOT NULL");
        builder.HasOne(e => e.Device)
            .WithMany(d => d.DeviceEmergencyEvents)
            .HasForeignKey(e => e.DeviceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
