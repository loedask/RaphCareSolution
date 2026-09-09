using RaphCare.Mobile.Kernel.Core.Common.Devices;
using Xunit;

namespace RaphCare.Mobile.Tests.Core.Common.Devices;

public sealed class WearableHeartDetectRulesTests
{
    [Fact]
    public void AcceptsNormalDetectBpmAndRejectsWearError()
    {
        Assert.True(WearableHeartDetectRules.ShouldAcceptHeartSample("STATE_HEART_DETECT", 72));
        Assert.True(WearableHeartDetectRules.ShouldAcceptHeartSample("STATE_HEART_NORMAL", 68));
        Assert.False(WearableHeartDetectRules.ShouldAcceptHeartSample("STATE_HEART_WEAR_ERROR", 72));
        Assert.False(WearableHeartDetectRules.ShouldAcceptHeartSample("STATE_HEART_DETECT", 5));
        Assert.False(WearableHeartDetectRules.ShouldAcceptHeartSample(null, null));
    }

    [Fact]
    public void BlockingStatusesHavePatientMessages()
    {
        // Regression: Measure timed out with no reading while the band reported wear/busy/battery.
        Assert.True(WearableHeartDetectRules.IsBlockingHeartStatus("STATE_HEART_BUSY"));
        Assert.Contains(
            "wrist",
            WearableHeartDetectRules.PatientMessageForHeartStatus("STATE_HEART_WEAR_ERROR")!,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "Charge",
            WearableHeartDetectRules.PatientMessageForHeartStatus("STATE_LOW_BATTERY")!,
            StringComparison.OrdinalIgnoreCase);
        Assert.Null(WearableHeartDetectRules.PatientMessageForHeartStatus("STATE_HEART_DETECT"));
    }
}
