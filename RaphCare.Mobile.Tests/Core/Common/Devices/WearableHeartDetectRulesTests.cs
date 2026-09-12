using RaphCare.Mobile.Kernel.Core.Common.Devices;
using Xunit;

namespace RaphCare.Mobile.Tests.Core.Common.Devices;

public sealed class WearableHeartDetectRulesTests
{
    [Fact]
    public void AcceptsOnlySettledHeartSampleNotInterimDetect()
    {
        // Regression: Measure accepted STATE_HEART_DETECT (early 70 bpm) then stopped;
        // watch later showed 85 under STATE_HEART_NORMAL.
        Assert.False(WearableHeartDetectRules.ShouldAcceptHeartSample("STATE_HEART_DETECT", 70));
        Assert.False(WearableHeartDetectRules.ShouldAcceptHeartSample("STATE_INIT", 70));
        Assert.True(WearableHeartDetectRules.ShouldAcceptHeartSample("STATE_HEART_NORMAL", 85));
        Assert.True(WearableHeartDetectRules.ShouldAcceptHeartSample(null, 72));
        Assert.False(WearableHeartDetectRules.ShouldAcceptHeartSample("STATE_HEART_WEAR_ERROR", 72));
        Assert.False(WearableHeartDetectRules.ShouldAcceptHeartSample("STATE_HEART_NORMAL", 5));
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
