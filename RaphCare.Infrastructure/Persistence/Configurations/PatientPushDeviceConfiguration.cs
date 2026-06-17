using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Patients;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public sealed class PatientPushDeviceConfiguration : IEntityTypeConfiguration<PatientPushDevice>
{
    public void Configure(EntityTypeBuilder<PatientPushDevice> builder)
    {
        builder.ToTable("PatientPushDevices");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.DeviceToken).IsRequired().HasMaxLength(512);
        builder.Property(e => e.Platform).IsRequired().HasMaxLength(32);
        builder.HasIndex(e => e.PatientId);
        builder.HasIndex(e => new { e.PatientId, e.DeviceToken }).IsUnique();
        builder.HasOne<Patient>()
            .WithMany()
            .HasForeignKey(e => e.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
