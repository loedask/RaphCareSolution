using RaphCare.Mobile.Kernel.Core.Common.Devices;
using Xunit;

namespace RaphCare.Mobile.Tests.Core.Common.Devices;

/// <summary>
/// Locks Connected-on-Devices vs Measure-ready mismatches that blocked Watch readings.
/// </summary>
public sealed class WearableLiveMeasureSessionRulesTests
{
    [Fact]
    public void BluetoothOnlyConnectedMustNotCountAsLiveMeasureSessionReady()
    {
        // Partner: Devices status Connected, Measure said connect first / Bluetooth only.
        Assert.False(DevicesBleSessionPolicy.IsExclusiveLiveMeasureSessionReady(
            preferExclusiveVendorSession: true,
            vendorSdkAvailable: true,
            coordinatorUsingVendorSession: false,
            bridgeSessionReady: false));
    }

    [Fact]
    public void VendorSessionFlagOrBridgeReadyMeansMeasureCanStartDetect()
    {
        Assert.True(DevicesBleSessionPolicy.IsExclusiveLiveMeasureSessionReady(
            preferExclusiveVendorSession: true,
            vendorSdkAvailable: true,
            coordinatorUsingVendorSession: true,
            bridgeSessionReady: false));
        Assert.True(DevicesBleSessionPolicy.IsExclusiveLiveMeasureSessionReady(
            preferExclusiveVendorSession: true,
            vendorSdkAvailable: true,
            coordinatorUsingVendorSession: false,
            bridgeSessionReady: true));
        Assert.True(DevicesBleSessionPolicy.ShouldAdoptBridgeVendorSession(
            coordinatorUsingVendorSession: false,
            bridgeSessionReady: true));
    }

    [Fact]
    public void OpeningDevicesWithConnectedButNoMeasureSessionMustHealReconnect()
    {
        // Auto-connect left ConnectedDeviceId without a Veepoo session; appear must reconnect.
        Assert.True(DevicesBleSessionPolicy.ShouldReconnectClaimedWatchOnAppear(
            hasLockedBluetoothMac: true,
            hasConnectedDeviceId: true,
            liveMeasureSessionReady: false));
        Assert.False(DevicesBleSessionPolicy.ShouldReconnectClaimedWatchOnAppear(
            hasLockedBluetoothMac: true,
            hasConnectedDeviceId: true,
            liveMeasureSessionReady: true));
    }

    [Fact]
    public void MeasureMustNotStealRadioFromActivePluginBleGattWithoutReleaseHandoff()
    {
        // Dual-stack handoff (GATT Connected → Veepoo mid-Measure) force-closed the app.
        Assert.False(DevicesBleSessionPolicy.ShouldEstablishVendorSessionForMeasure(
            enableVendorLiveMeasure: true,
            vendorSdkAvailable: true,
            alreadyUsingVendorSession: false,
            hasBluetoothMac: true,
            pluginBleGattConnected: true,
            mayReleasePluginBleThenVendorHandshake: false));
        Assert.True(DevicesBleSessionPolicy.ShouldEstablishVendorSessionForMeasure(
            enableVendorLiveMeasure: true,
            vendorSdkAvailable: true,
            alreadyUsingVendorSession: false,
            hasBluetoothMac: true,
            pluginBleGattConnected: false,
            mayReleasePluginBleThenVendorHandshake: false));
    }

    [Fact]
    public void ProbeBuildMayReleasePluginBleThenVendorHandshakeOnMeasure()
    {
        // Regression: Huawei normal Scan → Connect → Measure said Bluetooth only while
        // vendor-scan probe Measure worked. Probe builds release GATT then handshake.
        Assert.True(DevicesBleSessionPolicy.MayReleasePluginBleThenVendorHandshakeForMeasure);
        Assert.True(DevicesBleSessionPolicy.ShouldEstablishVendorSessionForMeasure(
            enableVendorLiveMeasure: true,
            vendorSdkAvailable: true,
            alreadyUsingVendorSession: false,
            hasBluetoothMac: true,
            pluginBleGattConnected: true,
            mayReleasePluginBleThenVendorHandshake: true));
    }

    [Fact]
    public void FreshMeasureSessionMustNotCallStopDetectBeforeStartDetect()
    {
        // "RaphCare keeps stopping" after Measure called stopDetect* with no prior startDetect*.
        Assert.True(DevicesBleSessionPolicy.MustNotCallVendorStopDetectWithoutStart);
        Assert.False(DevicesBleSessionPolicy.ShouldInvokeVendorStopDetect(detectStarted: false));
        Assert.True(DevicesBleSessionPolicy.ShouldInvokeVendorStopDetect(detectStarted: true));
    }
}
