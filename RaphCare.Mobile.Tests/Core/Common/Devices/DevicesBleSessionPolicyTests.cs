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
    }
}
