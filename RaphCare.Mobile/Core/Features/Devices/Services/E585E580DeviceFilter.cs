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
        // OEM Device Info may show ET580 / ET585; advertisement names often include either form.
        // "ET580".Contains("E580") is false, so match both spellings explicitly.
        return deviceName.Contains("ET580", StringComparison.OrdinalIgnoreCase)
               || deviceName.Contains("ET585", StringComparison.OrdinalIgnoreCase)
               || deviceName.Contains("E580", StringComparison.OrdinalIgnoreCase)
               || deviceName.Contains("E585", StringComparison.OrdinalIgnoreCase)
               || deviceName.Contains("YSC", StringComparison.OrdinalIgnoreCase)
               || deviceName.Contains("Bracelet", StringComparison.OrdinalIgnoreCase)
               || deviceName.Contains("SmartBand", StringComparison.OrdinalIgnoreCase);
    }
}
