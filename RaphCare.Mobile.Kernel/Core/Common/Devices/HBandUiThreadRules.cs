namespace RaphCare.Mobile.Kernel.Core.Common.Devices;

/// <summary>
/// Threading rules for the Android Veepoo/Inuker stack used by Measure.
/// </summary>
public static class HBandUiThreadRules
{
    /// <summary>
    /// Vendor connect/notify callbacks are delivered on the Android main looper.
    /// Awaiting those callbacks while already occupying the main thread deadlocks Measure
    /// and Android kills the process (looks like tapping Measure closes the app).
    /// Only short JNI invokes may run on the main thread; waits must be off-main.
    /// </summary>
    public static bool VendorConnectMustAwaitCallbacksOffMainThread => true;
}
