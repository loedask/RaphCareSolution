namespace RaphCare.Mobile.Kernel.Core.Common.Devices;

/// <summary>
/// Rules for patient Devices Scan: which advertisements to show, and when to free the radio first.
/// </summary>
public static class WearableBleScanRules
{
    /// <summary>
    /// Plugin.BLE holding GATT can hide the watch from rediscovery. Release Plugin.BLE only
    /// before manual Scan. Do not tear down an exclusive Veepoo session here:
    /// <c>disconnectWatch</c> then <c>connectDevice</c> force-closes some phones.
    /// </summary>
    public static bool ShouldReleaseActiveLinksBeforeManualScan(bool pluginBleConnected) =>
        pluginBleConnected;

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

    /// <summary>
    /// A claimed Veepoo watch can be reachable by its locked MAC while it is not advertising
    /// to Plugin.BLE (for example while the vendor stack or H Band previously held the radio).
    /// Keep one saved-watch row available so Connect can use the vendor MAC path.
    /// </summary>
    public static bool ShouldShowClaimedWatchFallback(
        string? preferredMac,
        bool hasDiscoveredClaimedMac) =>
        BluetoothMacNormalizer.TryNormalize(preferredMac) is not null
        && !hasDiscoveredClaimedMac;

    /// <summary>
    /// Without a locked six-octet MAC, Nearby cannot show the Claimed wearable fallback
    /// and Connect-by-MAC cannot run. Surface that as claim data, not a radio failure.
    /// </summary>
    public static bool IsMissingClaimedBluetoothMacForFallback(string? claimedBluetoothMac) =>
        BluetoothMacNormalizer.TryNormalize(claimedBluetoothMac) is null;
}
