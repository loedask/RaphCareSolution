using Microsoft.EntityFrameworkCore;
using RaphCare.Domain.Devices;

namespace RaphCare.Infrastructure.Persistence;

public class DeviceDbContext(DbContextOptions<DeviceDbContext> options) : DbContext(options)
{
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<DeviceType> DeviceTypes => Set<DeviceType>();
    public DbSet<DeviceManufacturer> DeviceManufacturers => Set<DeviceManufacturer>();
    public DbSet<DeviceFirmware> DeviceFirmwares => Set<DeviceFirmware>();
    public DbSet<DeviceAssignment> DeviceAssignments => Set<DeviceAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new Configurations.DeviceConfiguration());
    }
}
