namespace RaphCare.Mobile.Kernel.Core.Common.Devices;

/// <summary>
/// Maps Veepoo <c>HeartData</c> / <c>EHeartStatus</c> callback fields into patient-safe Measure rules.
/// </summary>
public static class WearableHeartDetectRules
{
    /// <summary>
    /// True when <paramref name="bpm"/> is a usable live heart rate for the patient UI.
    /// Wear-error / low-battery statuses are rejected even if a number is present.
    /// Interim <c>STATE_HEART_DETECT</c> samples are rejected: Measure takes the first
    /// accepted sample and stops, so accepting DETECT locked an early value (Huawei: 70
    /// while the watch later showed 85 under <c>STATE_HEART_NORMAL</c>).
    /// </summary>
    public static bool ShouldAcceptHeartSample(string? heartStatusName, int? bpm)
    {
        if (bpm is not (>= 20 and <= 300))
            return false;

        if (IsBlockingHeartStatus(heartStatusName))
            return false;

        if (heartStatusName is "STATE_HEART_DETECT" or "STATE_INIT")
            return false;

        // Prefer NORMAL; allow null/unknown status so older firmwares still work.
        return heartStatusName is null
            or "STATE_HEART_NORMAL";
    }

    /// <summary>Statuses where Measure should stop and tell the patient what to fix.</summary>
    public static bool IsBlockingHeartStatus(string? heartStatusName) =>
        heartStatusName is "STATE_HEART_WEAR_ERROR"
            or "STATE_LOW_BATTERY"
            or "STATE_HEART_BUSY";

    /// <summary>Short patient-facing copy for blocking Veepoo heart statuses.</summary>
    public static string? PatientMessageForHeartStatus(string? heartStatusName) =>
        heartStatusName switch
        {
            "STATE_HEART_WEAR_ERROR" =>
                "Wear the watch snug on your wrist, then tap Measure again.",
            "STATE_LOW_BATTERY" =>
                "Charge the watch, then tap Measure again.",
            "STATE_HEART_BUSY" =>
                "The watch is busy with another reading. Wait a few seconds, then tap Measure again.",
            _ => null,
        };
}
