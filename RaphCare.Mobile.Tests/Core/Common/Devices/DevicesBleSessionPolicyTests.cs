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
    public void CanStopScanWhenUiShowsScanningEvenIfAdapterFlagStillFalse()
    {
        // Regression: StopScanCommand used only adapter.IsScanning, raised once before StartScan,
        // so Stop stayed disabled for the whole scan while devices still appeared.
        Assert.True(DevicesBleSessionPolicy.CanStopScan(isScanningUi: true, adapterIsScanning: false));
        Assert.True(DevicesBleSessionPolicy.CanStopScan(isScanningUi: false, adapterIsScanning: true));
        Assert.False(DevicesBleSessionPolicy.CanStopScan(isScanningUi: false, adapterIsScanning: false));
    }

    [Fact]
    public void TryVendorSdkOnConnectStaysOffToAvoidFeatureNotSupportedToast()
    {
        // Regression: HBand/Inuker connect probed advertising and briefly toasted
        // "This feature is not supported" before GATT fallback still showed Connected.
        // Live vitals use MeasureLiveVitalsAsync on Watch readings instead.
        Assert.False(DevicesBleSessionPolicy.TryVendorSdkOnConnect);
    }

    [Fact]
    public void LiveMeasureTimeoutIsLongEnoughForWatchTapToTest()
    {
        Assert.True(DevicesBleSessionPolicy.LiveMeasureTimeout >= TimeSpan.FromSeconds(30));
        Assert.True(DevicesBleSessionPolicy.VendorHandshakeTimeout >= TimeSpan.FromSeconds(40));
        Assert.True(DevicesBleSessionPolicy.PostGattDisconnectSettle >= TimeSpan.FromMilliseconds(2000));
        Assert.True(DevicesBleSessionPolicy.VendorConnectMaxAttempts >= 3);
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
}
