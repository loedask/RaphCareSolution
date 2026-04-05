using Microsoft.EntityFrameworkCore;
using RaphCare.Domain.Devices;
using RaphCare.Infrastructure.Persistence.Configurations;

namespace RaphCare.Persistence;

/// <summary>
/// Bounded context: Device registry and assignments.
/// </summary>
public class DeviceDbContext : DbContext
{
    public DeviceDbContext(DbContextOptions<DeviceDbContext> options) : base(options) { }

    public DbSet<Device> Devices => Set<Device>();
    public DbSet<DeviceType> DeviceTypes => Set<DeviceType>();
    public DbSet<DeviceManufacturer> DeviceManufacturers => Set<DeviceManufacturer>();
    public DbSet<DeviceFirmware> DeviceFirmwares => Set<DeviceFirmware>();
    public DbSet<DeviceAssignment> DeviceAssignments => Set<DeviceAssignment>();
    public DbSet<DeviceReading> DeviceReadings => Set<DeviceReading>();
    public DbSet<DeviceEmergencyEvent> DeviceEmergencyEvents => Set<DeviceEmergencyEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new DeviceConfiguration());
        modelBuilder.ApplyConfiguration(new DeviceEmergencyEventConfiguration());
        modelBuilder.ApplyConfiguration(new DeviceReadingConfiguration());
        modelBuilder.ApplyPersistenceConventions();
    }
}
