namespace RaphCare.Mobile.Kernel.Core.Common.Devices;

/// <summary>
/// Survives a native Veepoo abort: write the last Connect step to disk before risky JNI,
/// then show a patient message on the next app open when the step never completed.
/// </summary>
public static class VendorConnectCrashProbeRules
{
    public const string StepInit1 = "INIT-1";
    public const string StepInit2 = "INIT-2";
    public const string StepInit3 = "INIT-3";
    public const string StepConnect1 = "CONNECT-1";
    public const string StepConnect2 = "CONNECT-2";
    public const string StepConnectInvoke = "CONNECT-INVOKE";
    public const string StepConnect3 = "CONNECT-3";
    public const string StepHandshakeOk = "HANDSHAKE-OK";

    /// <summary>
    /// Incomplete steps that mean the process likely died mid-vendor Connect.
    /// </summary>
    public static bool ShouldReportIncompleteStep(string? step) =>
        step is StepInit1
            or StepInit2
            or StepConnect1
            or StepConnect2
            or StepConnectInvoke;

    /// <summary>
    /// Successful markers clear the probe; do not report them after relaunch.
    /// </summary>
    public static bool ClearsProbe(string? step) =>
        step is StepInit3 or StepConnect3 or StepHandshakeOk;

    /// <summary>Patient-facing copy after relaunch. Includes the step code for engineers.</summary>
    public static string PatientMessageForIncompleteStep(string step)
    {
        var detail = step switch
        {
            StepInit1 or StepInit2 =>
                "The app closed while starting the watch SDK (" + step + ").",
            StepConnect1 =>
                "The app closed while checking the watch link (" + step + ").",
            StepConnect2 or StepConnectInvoke =>
                "The app closed while connecting to the watch through the watch SDK (" + step
                + "). That usually means the native connect call aborted.",
            _ => "The app closed during watch Connect (" + step + ")."
        };

        return detail
               + " You can keep using Connect on build 1.8.42. Tell your engineer this step code.";
    }
}
