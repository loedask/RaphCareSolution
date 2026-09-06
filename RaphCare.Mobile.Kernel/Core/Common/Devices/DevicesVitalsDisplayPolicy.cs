namespace RaphCare.Mobile.Kernel.Core.Common.Devices;

/// <summary>What the patient Devices "Last reading" card should show.</summary>
public static class DevicesVitalsDisplayPolicy
{
    /// <summary>
    /// Raw GATT hex is vendor noise for E580/E585 sample bands. Keep it off the patient UI
    /// unless the debug "show all BLE" toggle is on.
    /// </summary>
    public static bool ShowRawHexToPatient(bool showAllDevicesDebug) => showAllDevicesDebug;

    /// <summary>True when we have a parsed heart rate or oxygen value to display.</summary>
    public static bool HasPatientFacingReading(int? heartRateBpm, decimal? spO2Percent) =>
        heartRateBpm.HasValue || spO2Percent.HasValue;
}
