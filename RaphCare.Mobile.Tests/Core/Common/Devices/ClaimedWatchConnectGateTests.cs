using RaphCare.Mobile.Kernel.Core.Common.Devices;
using Xunit;

namespace RaphCare.Mobile.Tests.Core.Common.Devices;

public sealed class ClaimedWatchConnectGateTests
{
    [Fact]
    public void AllowsConnectWhenNoClaimedMacAllowsAnyPeripheral()
    {
        Assert.True(ClaimedWatchConnectGate.AllowsConnect(null, "AA:BB:CC:DD:EE:FF"));
        Assert.True(ClaimedWatchConnectGate.AllowsConnect("", "11:22:33:44:55:66"));
    }

    [Fact]
    public void AllowsConnectWhenClaimedMacSetRejectsOtherBand()
    {
        // Same shape as the partner reject: claimed ET585 MAC vs nearby ET580.
        Assert.False(ClaimedWatchConnectGate.AllowsConnect(
            "6F:9A:C8:4C:E4:45",
            "AA:BB:CC:DD:EE:01"));
    }

    [Fact]
    public void AllowsConnectWhenClaimedMacSetAcceptsMatchingBand()
    {
        Assert.True(ClaimedWatchConnectGate.AllowsConnect(
            "6F:9A:C8:4C:E4:45",
            "6f9ac84ce445"));
    }

    [Fact]
    public void AllowsConnectWhenClaimedMacSetRejectsPeripheralWithoutMac()
    {
        Assert.False(ClaimedWatchConnectGate.AllowsConnect("6F:9A:C8:4C:E4:45", null));
    }
}
