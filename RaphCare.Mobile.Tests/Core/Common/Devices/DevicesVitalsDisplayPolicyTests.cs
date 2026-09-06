using RaphCare.Mobile.Kernel.Core.Common.Devices;
using Xunit;

namespace RaphCare.Mobile.Tests.Core.Common.Devices;

public sealed class DevicesVitalsDisplayPolicyTests
{
    [Fact]
    public void ShowRawHexToPatientIsOffUnlessDebugToggle()
    {
        // Regression: Last reading showed Raw: 01130000 after GATT connect with no parsed vitals.
        Assert.False(DevicesVitalsDisplayPolicy.ShowRawHexToPatient(showAllDevicesDebug: false));
        Assert.True(DevicesVitalsDisplayPolicy.ShowRawHexToPatient(showAllDevicesDebug: true));
    }

    [Fact]
    public void HasPatientFacingReadingRequiresParsedHrOrSpo2()
    {
        Assert.False(DevicesVitalsDisplayPolicy.HasPatientFacingReading(null, null));
        Assert.True(DevicesVitalsDisplayPolicy.HasPatientFacingReading(72, null));
        Assert.True(DevicesVitalsDisplayPolicy.HasPatientFacingReading(null, 98m));
    }
}
