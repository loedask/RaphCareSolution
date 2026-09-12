using RaphCare.Mobile.Kernel.Core.Common.Devices;
using Xunit;

namespace RaphCare.Mobile.Tests.Core.Common.Devices;

public sealed class VeepooSdkInitRulesTests
{
    [Fact]
    public void PreferGetManagerInstanceWithContextIsRequired()
    {
        // Regression: bare getInstance() leaves BluetoothClient null; connectDevice NPEs.
        Assert.True(VeepooSdkInitRules.PreferGetManagerInstanceWithContext);
    }

    [Fact]
    public void ShouldCallInitOnlyWhenBluetoothClientMissing()
    {
        Assert.True(VeepooSdkInitRules.ShouldCallInitWhenBluetoothClientMissing(false));
        Assert.False(VeepooSdkInitRules.ShouldCallInitWhenBluetoothClientMissing(true));
    }

    [Fact]
    public void MacOnlyConnectDeviceOverloadFlagSelectsThreeArgPath()
    {
        // javap: 3-arg connectDevice forwards to 4-arg with name "none".
        // Probe 1.8.53 turns PreferOfficialMacOnly on for WAIT-CONNECT A/B.
        Assert.True(DevicesBleSessionPolicy.PreferOfficialMacOnlyConnectDeviceOverload);
        Assert.True(VeepooSdkInitRules.PreferMacPlusNameConnectDeviceOverload(
            preferOfficialMacOnlyConnectDeviceOverload: false));
        Assert.False(VeepooSdkInitRules.PreferMacPlusNameConnectDeviceOverload(
            preferOfficialMacOnlyConnectDeviceOverload: true));
    }

    [Fact]
    public void MustNotProbeNativeConnectedWhenBluetoothClientMissing()
    {
        Assert.False(VeepooSdkInitRules.ShouldProbeNativeConnectedLink(false));
        Assert.True(VeepooSdkInitRules.ShouldProbeNativeConnectedLink(true));
    }

    [Fact]
    public void VendorNativeScanProbeIsGatedSeparatelyFromHybridExclusive()
    {
        Assert.True(VeepooSdkInitRules.ShouldUseVendorNativeScan(true));
        Assert.False(VeepooSdkInitRules.ShouldUseVendorNativeScan(false));
        Assert.True(VeepooSdkInitRules.ShouldAcceptVendorScanResult(
            deviceName: "ET585",
            deviceMac: "AA:BB:CC:DD:EE:FF",
            preferredMac: null,
            nameLooksLikeE580Style: true));
        Assert.True(VeepooSdkInitRules.ShouldAcceptVendorScanResult(
            deviceName: null,
            deviceMac: "AA:BB:CC:DD:EE:FF",
            preferredMac: "AA:BB:CC:DD:EE:FF",
            nameLooksLikeE580Style: false));
        Assert.False(VeepooSdkInitRules.ShouldAcceptVendorScanResult(
            deviceName: "Phone",
            deviceMac: "11:22:33:44:55:66",
            preferredMac: "AA:BB:CC:DD:EE:FF",
            nameLooksLikeE580Style: false));
    }

    [Fact]
    public void TimedStartScanOverloadStaysOffToAvoidDoubleStop()
    {
        // Timed startScanDevice(int, …) may schedule its own stop; keep untimed only.
        Assert.False(VeepooSdkInitRules.ShouldUseTimedStartScanOverload);
    }
}
