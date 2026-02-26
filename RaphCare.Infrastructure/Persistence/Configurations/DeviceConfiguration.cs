using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Devices;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("Devices");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.SerialNumber).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Model).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(50);
        builder.HasIndex(e => e.ClinicId);
        builder.HasIndex(e => e.SerialNumber).IsUnique();
        builder.HasIndex(e => e.DeviceTypeId);
        builder.HasIndex(e => e.DeviceManufacturerId);
        builder.HasOne(e => e.DeviceType).WithMany().HasForeignKey(e => e.DeviceTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.DeviceManufacturer).WithMany().HasForeignKey(e => e.DeviceManufacturerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.DeviceFirmware).WithMany().HasForeignKey(e => e.DeviceFirmwareId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.DeviceAssignments).WithOne(a => a.Device).HasForeignKey(a => a.DeviceId).OnDelete(DeleteBehavior.Restrict);
    }
}
