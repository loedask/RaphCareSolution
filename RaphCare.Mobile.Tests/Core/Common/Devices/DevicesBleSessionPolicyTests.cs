using RaphCare.Mobile.Kernel.Core.Common.Devices;
using Xunit;

namespace RaphCare.Mobile.Tests.Core.Common.Devices;

public sealed class DevicesBleSessionPolicyTests
{
    [Fact]
    public void LeavingDevicesPageMustNotDisconnectBle()
    {
        // Regression: OnDisappearing used to DisconnectAsync, so returning from Home showed disconnected.
        Assert.False(DevicesBleSessionPolicy.DisconnectWhenLeavingDevicesPage);
    }

    [Fact]
    public void ClosingTheAppMustReconnectClaimedWatchWhenDevicesAppears()
    {
        // Regression: Connect after force-closing the app required a fresh Scan, and
        // disconnect-then-connect on the claimed MAC killed the process.
        Assert.True(DevicesBleSessionPolicy.ReconnectClaimedWatchWhenDevicesAppears);
        Assert.True(DevicesBleSessionPolicy.ShouldReconnectClaimedWatchOnAppear(
            hasLockedBluetoothMac: true,
            hasConnectedDeviceId: false,
            liveMeasureSessionReady: false));
        Assert.False(DevicesBleSessionPolicy.ShouldReconnectClaimedWatchOnAppear(
            hasLockedBluetoothMac: true,
            hasConnectedDeviceId: true,
            liveMeasureSessionReady: true));
        Assert.True(DevicesBleSessionPolicy.ShouldReconnectClaimedWatchOnAppear(
            hasLockedBluetoothMac: true,
            hasConnectedDeviceId: true,
            liveMeasureSessionReady: false));
        Assert.False(DevicesBleSessionPolicy.ShouldReconnectClaimedWatchOnAppear(
            hasLockedBluetoothMac: false,
            hasConnectedDeviceId: false,
            liveMeasureSessionReady: false));
        Assert.True(DevicesBleSessionPolicy.ShouldUsePluginBleReconnectWhenVendorUnavailable(
            vendorSdkAvailable: false));
        Assert.False(DevicesBleSessionPolicy.ShouldUsePluginBleReconnectWhenVendorUnavailable(
            vendorSdkAvailable: true));
        Assert.True(DevicesBleSessionPolicy.ClaimedWatchReconnectScanTimeout >= TimeSpan.FromSeconds(8));
    }

    [Fact]
    public void DevicesAppearMustNotRequestBluetoothPermissionThenConnect()
    {
        // Regression: OnAppearing RequestAsync → grant → Veepoo connectDevice force-closed
        // the app as soon as the patient accepted Nearby devices.
        Assert.False(DevicesBleSessionPolicy.ShouldRequestBluetoothPermissionOnDevicesAppear);
        Assert.True(DevicesBleSessionPolicy.PostPermissionGrantSettle >= TimeSpan.FromMilliseconds(500));
        Assert.False(DevicesBleSessionPolicy.ShouldReconnectClaimedWatchOnAppearWithPermission(
            nearbyDevicesPermissionGranted: false,
            hasLockedBluetoothMac: true,
            hasConnectedDeviceId: false,
            liveMeasureSessionReady: false));
        Assert.True(DevicesBleSessionPolicy.ShouldReconnectClaimedWatchOnAppearWithPermission(
            nearbyDevicesPermissionGranted: true,
            hasLockedBluetoothMac: true,
            hasConnectedDeviceId: false,
            liveMeasureSessionReady: false));
    }

    [Fact]
    public void ConnectMustReuseVendorSessionOnTheSameMacInsteadOfDisconnectThenConnect()
    {
        const string mac = "6F:9A:C8:4C:E4:45";
        Assert.True(DevicesBleSessionPolicy.ShouldReuseExistingVendorSession(
            vendorSessionReady: true,
            sessionMac: mac,
            targetMac: "6f9ac84ce445"));
        Assert.False(DevicesBleSessionPolicy.ShouldReuseExistingVendorSession(
            vendorSessionReady: false,
            sessionMac: mac,
            targetMac: mac));
        Assert.False(DevicesBleSessionPolicy.ShouldReuseExistingVendorSession(
            vendorSessionReady: true,
            sessionMac: mac,
            targetMac: "AA:BB:CC:DD:EE:01"));
    }

    [Fact]
    public void ExclusiveModeOffMeansDisconnectDoesNotRetainVendorSession()
    {
        // Exclusive Connect is off because connectDevice (mac-only and mac+name) force-closes.
        Assert.False(DevicesBleSessionPolicy.PreferExclusiveVendorSession);
        Assert.False(DevicesBleSessionPolicy.KeepVendorSessionAliveAfterUserDisconnect);
    }

    [Fact]
    public void AutoReconnectMustCoolDownAfterFailure()
    {
        // Regression: Devices appear kept calling connectDevice after a timeout, leaving
        // "Reconnecting..." and a hung radio until the patient force-closed the app.
        var failedAt = new DateTimeOffset(2026, 9, 8, 20, 0, 0, TimeSpan.Zero);
        Assert.True(DevicesBleSessionPolicy.ShouldSkipClaimedWatchReconnectAfterRecentFailure(
            failedAt,
            failedAt.AddMinutes(1)));
        Assert.False(DevicesBleSessionPolicy.ShouldSkipClaimedWatchReconnectAfterRecentFailure(
            failedAt,
            failedAt.AddMinutes(2)));
        Assert.False(DevicesBleSessionPolicy.ShouldSkipClaimedWatchReconnectAfterRecentFailure(
            null,
            failedAt));
    }

    [Fact]
    public void MustNotWaitAfterPluginBleReleaseWhenAlreadyDisconnected()
    {
        Assert.False(DevicesBleSessionPolicy.ShouldWaitAfterReleasingPluginBle(pluginBleWasConnected: false));
        Assert.True(DevicesBleSessionPolicy.ShouldWaitAfterReleasingPluginBle(pluginBleWasConnected: true));
    }

    [Fact]
    public void CanStopScanWhenUiShowsScanningEvenIfAdapterFlagStillFalse()
    {
        // Regression: StopScanCommand used only adapter.IsScanning, raised once before StartScan,
        // so Stop stayed disabled for the whole scan while devices still appeared.
        Assert.True(DevicesBleSessionPolicy.CanStopScan(isScanningUi: true, adapterIsScanning: false));
        Assert.True(DevicesBleSessionPolicy.CanStopScan(isScanningUi: false, adapterIsScanning: true));
        Assert.False(DevicesBleSessionPolicy.CanStopScan(isScanningUi: false, adapterIsScanning: false));
    }

    [Fact]
    public void ClaimedWatchConnectMustUsePluginBleWhileVendorConnectDeviceCrashes()
    {
        // Regression: Scan found ET585; exclusive connectDevice (including wiki mac-only)
        // force-closed the app. Keep Veepoo Connect off until a safe native path exists.
        Assert.False(DevicesBleSessionPolicy.PreferExclusiveVendorSession);
        Assert.False(DevicesBleSessionPolicy.TryVendorSdkOnConnect);
        Assert.False(DevicesBleSessionPolicy.EnableVendorLiveMeasure);
        Assert.True(DevicesBleSessionPolicy.PreferOfficialMacOnlyConnectDeviceOverload);
        Assert.False(DevicesBleSessionPolicy.RestorePluginBleAfterMeasure);
        Assert.False(DevicesBleSessionPolicy.EnableVendorSpo2DuringMeasure);
    }

    [Fact]
    public void LiveMeasureTimeoutIsLongEnoughForWatchTapToTest()
    {
        Assert.True(DevicesBleSessionPolicy.LiveMeasureTimeout >= TimeSpan.FromSeconds(30));
        Assert.True(DevicesBleSessionPolicy.VendorHandshakeTimeout >= TimeSpan.FromSeconds(40));
        Assert.True(DevicesBleSessionPolicy.PostGattDisconnectSettle >= TimeSpan.FromMilliseconds(2000));
        Assert.True(DevicesBleSessionPolicy.VendorConnectMaxAttempts >= 2);
        Assert.True(DevicesBleSessionPolicy.LiveMeasureOverallTimeout
                    > DevicesBleSessionPolicy.LiveMeasureTimeout);
    }

    [Fact]
    public void HBandCodeMinusTwoExplainsRadioBusyNotBareCodeOnly()
    {
        // Regression: Measure showed only "HBand connect failed (code -2)" with no recovery hint.
        var text = HBandConnectFailureMessages.ForCode(HBandConnectFailureMessages.RequestCanceled);
        Assert.Contains("code -2", text, StringComparison.Ordinal);
        Assert.Contains("busy", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Measure", text, StringComparison.OrdinalIgnoreCase);
        Assert.True(HBandConnectFailureMessages.IsRadioBusyCancel(text));
        Assert.False(HBandConnectFailureMessages.IsRadioBusyCancel("HBand connect failed (code -7)."));
    }

    [Fact]
    public void VendorConnectMustAwaitCallbacksOffMainThread()
    {
        // Regression: ConnectAndHandshake awaited connect/notify on the UI thread via
        // MainThread.InvokeOnMainThreadAsync(async () => await connectTcs), so Inuker
        // connectState could not run on the looper and Measure killed the app.
        Assert.True(HBandUiThreadRules.VendorConnectMustAwaitCallbacksOffMainThread);
    }

    [Fact]
    public void MustNotCallVendorDisconnectWithoutSession()
    {
        // Regression: Measure called disconnectWatch/stopDetect on a cold VPOperateManager,
        // which toasted "This feature is not supported" and then killed the process.
        Assert.True(DevicesBleSessionPolicy.MustNotCallVendorDisconnectWithoutSession);
        Assert.False(DevicesBleSessionPolicy.ShouldInvokeVendorDisconnect(
            sessionReady: false,
            connectStarted: false));
        Assert.True(DevicesBleSessionPolicy.ShouldInvokeVendorDisconnect(
            sessionReady: true,
            connectStarted: false));
        Assert.True(DevicesBleSessionPolicy.ShouldInvokeVendorDisconnect(
            sessionReady: false,
            connectStarted: true));
        Assert.True(DevicesBleSessionPolicy.PostGattDisconnectSettle >= TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MeasureMustNotStartSpo2WhileHeartDetectStillRuns()
    {
        // Regression: after the first HR sample, Measure called startDetectSPO2H while
        // startDetectHeart was still active. Veepoo killed the process mid-measure.
        Assert.False(DevicesBleSessionPolicy.EnableVendorSpo2DuringMeasure);
    }

    [Fact]
    public void MustWaitAfterVendorDisconnectBeforeReconnect()
    {
        // Regression: Disconnect then Connect on Claimed wearable force-closed the app
        // when Veepoo connectDevice ran before disconnectWatch finished tearing down.
        Assert.Equal(
            DevicesBleSessionPolicy.PostGattDisconnectSettle,
            DevicesBleSessionPolicy.PostVendorDisconnectSettle);
        Assert.Equal(
            TimeSpan.Zero,
            DevicesBleSessionPolicy.RemainingVendorReconnectSettle(null, DateTimeOffset.UtcNow));

        var disconnectedAt = new DateTimeOffset(2026, 9, 8, 12, 0, 0, TimeSpan.Zero);
        Assert.Equal(
            TimeSpan.FromSeconds(5),
            DevicesBleSessionPolicy.RemainingVendorReconnectSettle(
                disconnectedAt,
                disconnectedAt));
        Assert.Equal(
            TimeSpan.FromSeconds(2),
            DevicesBleSessionPolicy.RemainingVendorReconnectSettle(
                disconnectedAt,
                disconnectedAt.AddSeconds(3)));
        Assert.Equal(
            TimeSpan.Zero,
            DevicesBleSessionPolicy.RemainingVendorReconnectSettle(
                disconnectedAt,
                disconnectedAt.AddSeconds(5)));
    }

    [Fact]
    public void MissingBluetoothScanSecurityExceptionIsRecognized()
    {
        // Regression: Scan showed raw "Need android.permission.BLUETOOTH_SCAN ... registerScanner"
        // instead of a patient-facing Nearby devices prompt after auto-reconnect / Scan.
        Assert.True(BlePermissionFailure.IsMissingNearbyDevicesPermission(
            "Need android.permission.BLUETOOTH_SCAN permission for android.content.AttributionSource@b92af2e9: GattService registerScanner"));
        Assert.False(BlePermissionFailure.IsMissingNearbyDevicesPermission("Watch SDK connect failed."));
    }
}
