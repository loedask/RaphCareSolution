namespace RaphCare.Mobile.Core.Features.Devices.Services;

/// <summary>
/// Filters scan results to bracelets commonly sold as E580 / E585 (OEM names vary).
/// Extend when you have manufacturer data or exact GATT service UUIDs from the vendor.
/// </summary>
public static class E585E580DeviceFilter
{
    /// <summary>Returns true if the advertised name suggests an E580/E585-class bracelet.</summary>
    public static bool Matches(string? deviceName)
    {
        if (string.IsNullOrWhiteSpace(deviceName))
            return false;
        return deviceName.Contains("E580", StringComparison.OrdinalIgnoreCase)
               || deviceName.Contains("E585", StringComparison.OrdinalIgnoreCase)
               || deviceName.Contains("YSC", StringComparison.OrdinalIgnoreCase)
               || deviceName.Contains("Bracelet", StringComparison.OrdinalIgnoreCase)
               || deviceName.Contains("SmartBand", StringComparison.OrdinalIgnoreCase);
    }
}
