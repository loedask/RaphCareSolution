using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Devices;

/// <summary>
/// Physical monitoring device. Aggregate root for high-volume ingestion.
/// </summary>
public class Device : AggregateRoot
{
    public Guid ClinicId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public Guid DeviceTypeId { get; set; }
    public Guid DeviceManufacturerId { get; set; }
    public Guid? DeviceFirmwareId { get; set; }
    public string Model { get; set; } = string.Empty;
    /// <summary>Normalized Bluetooth MAC (<c>AA:BB:CC:DD:EE:FF</c>) when known; null until Ops stocks it or the patient locks it on first pair.</summary>
    public string? BluetoothMacAddress { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsAssigned { get; set; }
    public string Status { get; set; } = string.Empty;

    public DeviceType DeviceType { get; set; } = null!;
    public DeviceManufacturer DeviceManufacturer { get; set; } = null!;
    public DeviceFirmware? DeviceFirmware { get; set; }
    public ICollection<DeviceAssignment> DeviceAssignments { get; set; } = new List<DeviceAssignment>();
    public ICollection<DeviceReading> DeviceReadings { get; set; } = new List<DeviceReading>();
    public ICollection<DeviceEmergencyEvent> DeviceEmergencyEvents { get; set; } = new List<DeviceEmergencyEvent>();
    public ICollection<DeviceAlert> DeviceAlerts { get; set; } = new List<DeviceAlert>();
    public ICollection<DeviceCalibrationRecord> DeviceCalibrationRecords { get; set; } = new List<DeviceCalibrationRecord>();
}
