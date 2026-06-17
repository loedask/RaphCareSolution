using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Devices;

namespace RaphCare.Infrastructure.Persistence.Configurations;

/// <summary>TPH mapping for <see cref="DeviceReading"/> hierarchy (device bounded context).</summary>
public class DeviceReadingConfiguration : IEntityTypeConfiguration<DeviceReading>
{
    public void Configure(EntityTypeBuilder<DeviceReading> builder)
    {
        builder.ToTable("DeviceReadings");

        builder.HasDiscriminator<string>("Discriminator")
            .HasValue<HeartRateReading>("HeartRate")
            .HasValue<PulseOximeterReading>("PulseOximeter")
            .HasValue<BloodPressureReading>("BloodPressure")
            .HasValue<GlucoseReading>("Glucose")
            .HasValue<ECGReading>("ECG")
            .HasValue<WeightReading>("Weight");

        builder.Property(e => e.ReadingType).IsRequired().HasMaxLength(64);
        builder.Property(e => e.Unit).IsRequired().HasMaxLength(32);
        builder.HasIndex(e => e.DeviceId);
        builder.HasIndex(e => e.PatientId);
        builder.HasIndex(e => e.RecordedAt);

        builder.HasOne(e => e.Device)
            .WithMany(d => d.DeviceReadings)
            .HasForeignKey(e => e.DeviceId);
    }
}
