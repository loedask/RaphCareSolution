namespace RaphCare.Domain.Devices;

/// <summary>
/// Stable identifiers for seeded <see cref="DeviceType"/> and <see cref="DeviceManufacturer"/> rows
/// used when patients register BLE wearables (E580/E585-class) via the patient API.
/// </summary>
public static class KnownDeviceCatalogIds
{
    /// <summary>Wearable BLE band / watch (vitals from phone gateway).</summary>
    public static readonly Guid WearableBleDeviceTypeId = Guid.Parse("f8c3b2a1-4d5e-4f6a-9b0c-1d2e3f4a5b6c");

    /// <summary>Generic OEM / program fleet manufacturer placeholder.</summary>
    public static readonly Guid GenericOemManufacturerId = Guid.Parse("a7b6c5d4-e3f2-4a1b-9c8d-7e6f5a4b3c2d");
}
