using RaphCare.Mobile.Kernel.Core.Common.Devices;
using Xunit;

namespace RaphCare.Mobile.Tests.Core.Common.Devices;

/// <summary>
/// Locks the partner bug: auto-reconnect works, but manual Scan shows an empty Nearby list.
/// </summary>
public sealed class WearableBleScanRulesTests
{
    private const string ClaimedMac = "6F:9A:C8:4C:E4:45";

    [Fact]
    public void ManualScanWhileBluetoothOnlyConnectedMustReleaseTheHeldLink()
    {
        // Partner: Devices showed Connected / Bluetooth-only, Scan ran, Nearby stayed empty.
        // Android often hides a peripheral that Plugin.BLE or Veepoo still holds.
        Assert.True(WearableBleScanRules.ShouldReleaseActiveLinksBeforeManualScan(
            hasConnectedDeviceId: true,
            pluginBleConnected: false,
            vendorSessionActive: false));
        Assert.True(WearableBleScanRules.ShouldReleaseActiveLinksBeforeManualScan(
            hasConnectedDeviceId: false,
            pluginBleConnected: true,
            vendorSessionActive: false));
        Assert.True(WearableBleScanRules.ShouldReleaseActiveLinksBeforeManualScan(
            hasConnectedDeviceId: false,
            pluginBleConnected: false,
            vendorSessionActive: true));
        Assert.False(WearableBleScanRules.ShouldReleaseActiveLinksBeforeManualScan(
            hasConnectedDeviceId: false,
            pluginBleConnected: false,
            vendorSessionActive: false));
    }

    [Fact]
    public void ClaimedMacScanMustUsePermissivePluginBleFilterSoBlankNamesAreNotDroppedEarly()
    {
        // Regression: IncludeInFilteredScan in Plugin.BLE's deviceFilter ran before NativeDevice
        // MAC was readable, so blank-name claimed bands never raised DeviceDiscovered.
        Assert.True(WearableBleScanRules.UsePermissivePluginBleScanFilter(
            showAllDevices: false,
            hasPreferredMac: true));
        Assert.False(WearableBleScanRules.UsePermissivePluginBleScanFilter(
            showAllDevices: false,
            hasPreferredMac: false));
        Assert.True(WearableBleScanRules.UsePermissivePluginBleScanFilter(
            showAllDevices: true,
            hasPreferredMac: false));
    }

    [Fact]
    public void FilteredScanMustShowClaimedWatchEvenWhenAdvertisementNameIsBlank()
    {
        // Partner: E580/E585 name filter was on; claimed band advertised with a blank/OEM name
        // and never appeared under Nearby while auto-reconnect still worked by MAC.
        Assert.False(WearableBleScanRules.MatchesE580StyleName(null));
        Assert.False(WearableBleScanRules.MatchesE580StyleName(""));
        Assert.False(WearableBleScanRules.MatchesE580StyleName("   "));

        Assert.True(WearableBleScanRules.IncludeInFilteredScan(
            deviceName: null,
            deviceMac: ClaimedMac,
            preferredMac: "6f9ac84ce445",
            showAllDevices: false));
        Assert.True(WearableBleScanRules.IncludeInFilteredScan(
            deviceName: "",
            deviceMac: ClaimedMac,
            preferredMac: ClaimedMac,
            showAllDevices: false));
    }

    [Fact]
    public void FilteredScanMustNotShowUnrelatedDevicesWhenClaimedMacIsSet()
    {
        Assert.False(WearableBleScanRules.IncludeInFilteredScan(
            deviceName: "Pixel Buds",
            deviceMac: "AA:BB:CC:DD:EE:FF",
            preferredMac: ClaimedMac,
            showAllDevices: false));
    }

    [Theory]
    [InlineData("ET580")]
    [InlineData("ET585")]
    [InlineData("E580-ABC")]
    [InlineData("E585")]
    [InlineData("VP07")]
    [InlineData("H Band")]
    public void E580StyleNameFilterMustAcceptKnownBandNames(string name)
    {
        Assert.True(WearableBleScanRules.MatchesE580StyleName(name));
        Assert.True(WearableBleScanRules.IncludeInFilteredScan(
            deviceName: name,
            deviceMac: null,
            preferredMac: ClaimedMac,
            showAllDevices: false));
    }

    [Fact]
    public void ShowAllDevicesMustBypassNameAndMacFilters()
    {
        Assert.True(WearableBleScanRules.IncludeInFilteredScan(
            deviceName: "Anything",
            deviceMac: "11:22:33:44:55:66",
            preferredMac: ClaimedMac,
            showAllDevices: true));
    }

    [Fact]
    public void ClaimedWatchFallbackMustRemainVisibleWhenPluginBleSeesNoMatchingAdvertisement()
    {
        // A Veepoo MAC connect can work while the band is not advertising to Plugin.BLE.
        Assert.True(WearableBleScanRules.ShouldShowClaimedWatchFallback(ClaimedMac, false));
        Assert.False(WearableBleScanRules.ShouldShowClaimedWatchFallback(ClaimedMac, true));
        Assert.False(WearableBleScanRules.ShouldShowClaimedWatchFallback(null, false));
    }
}
