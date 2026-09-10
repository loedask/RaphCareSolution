using RaphCare.Mobile.Kernel.Core.Common.Devices;
using Xunit;

namespace RaphCare.Mobile.Tests.Core.Common.Devices;

public sealed class VeepooSdkInitRulesTests
{
    [Fact]
    public void PreferGetManagerInstanceWithContext_IsRequired()
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
    public void MacOnlyConnectDeviceOverloadIsNotASaferPath()
    {
        // javap: 3-arg connectDevice forwards to 4-arg with name "none".
        Assert.False(DevicesBleSessionPolicy.PreferOfficialMacOnlyConnectDeviceOverload);
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
}
