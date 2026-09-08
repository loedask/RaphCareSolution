namespace RaphCare.Mobile.Kernel.Core.Common.Devices;

/// <summary>
/// Rules for patient Devices Scan: which advertisements to show, and when to free the radio first.
/// </summary>
public static class WearableBleScanRules
{
    /// <summary>
    /// Plugin.BLE / Veepoo holding the watch often blocks rediscovery. Release before manual Scan
    /// whenever Devices still shows a connected id, an active GATT link, or a vendor session.
    /// </summary>
    public static bool ShouldReleaseActiveLinksBeforeManualScan(
        bool hasConnectedDeviceId,
        bool pluginBleConnected,
        bool vendorSessionActive) =>
        hasConnectedDeviceId || pluginBleConnected || vendorSessionActive;

    /// <summary>
    /// When a claimed MAC is known, Plugin.BLE must not drop advertisements in its early filter.
    /// NativeDevice MAC is often unread there, so blank-name bands never fire DeviceDiscovered.
    /// Filter later in OnDeviceDiscovered once the address is readable.
    /// </summary>
    public static bool UsePermissivePluginBleScanFilter(
        bool showAllDevices,
        bool hasPreferredMac) =>
        showAllDevices || hasPreferredMac;

    /// <summary>True when the advertised name looks like an E580 / E585-class band.</summary>
    public static bool MatchesE580StyleName(string? deviceName)
    {
        if (string.IsNullOrWhiteSpace(deviceName))
            return false;

        // OEM Device Info may show ET580 / ET585; advertisements often use either form.
        // "ET580".Contains("E580") is false, so match both spellings explicitly.
        return deviceName.Contains("ET580", StringComparison.OrdinalIgnoreCase)
               || deviceName.Contains("ET585", StringComparison.OrdinalIgnoreCase)
               || deviceName.Contains("E580", StringComparison.OrdinalIgnoreCase)
               || deviceName.Contains("E585", StringComparison.OrdinalIgnoreCase)
               || deviceName.Contains("YSC", StringComparison.OrdinalIgnoreCase)
               || deviceName.Contains("Bracelet", StringComparison.OrdinalIgnoreCase)
               || deviceName.Contains("SmartBand", StringComparison.OrdinalIgnoreCase)
               || deviceName.Contains("HBand", StringComparison.OrdinalIgnoreCase)
               || deviceName.Contains("H Band", StringComparison.OrdinalIgnoreCase)
               || deviceName.Contains("Veepoo", StringComparison.OrdinalIgnoreCase)
               || deviceName.StartsWith("VP", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Filtered Scan includes E580-style names and the patient's claimed MAC (even when the
    /// advertisement name is blank or OEM-only). Show-all bypasses the name filter.
    /// </summary>
    public static bool IncludeInFilteredScan(
        string? deviceName,
        string? deviceMac,
        string? preferredMac,
        bool showAllDevices) =>
        showAllDevices
        || MatchesE580StyleName(deviceName)
        || (preferredMac is not null
            && BluetoothMacNormalizer.EqualsNormalized(deviceMac, preferredMac));
}
