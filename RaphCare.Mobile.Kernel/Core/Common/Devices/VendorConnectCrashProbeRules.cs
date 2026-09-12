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
    public const string StepScan1 = "SCAN-1";
    public const string StepScanInvoke = "SCAN-INVOKE";
    public const string StepScanResult = "SCAN-RESULT";
    public const string StepScanStop = "SCAN-STOP";
    public const string StepScanProxyFailed = "SCAN-PROXY-FAILED";
    public const string StepSettle1 = "SETTLE-1";
    public const string StepConnect1 = "CONNECT-1";
    public const string StepConnect2 = "CONNECT-2";
    public const string StepConnectInvoke = "CONNECT-INVOKE";
    public const string StepConnect3 = "CONNECT-3";
    public const string StepWaitConnect = "WAIT-CONNECT";
    public const string StepWaitNotify = "WAIT-NOTIFY";
    public const string StepPwd1 = "PWD-1";
    public const string StepPerson1 = "PERSON-1";
    public const string StepHandshakeOk = "HANDSHAKE-OK";

    /// <summary>
    /// Incomplete steps that mean the process likely died mid-vendor Scan or Connect.
    /// Includes <see cref="StepScanStop"/>: stopping scan is housekeeping before the risky
    /// <c>connectDevice</c> call, not proof Connect will survive.
    /// Includes <see cref="StepConnect3"/>: <c>connectDevice</c> returning is not handshake done
    /// (phone trail from 1.8.49 died after CONNECT-3 with no HANDSHAKE-OK).
    /// </summary>
    public static bool ShouldReportIncompleteStep(string? step) =>
        step is StepInit1
            or StepInit2
            or StepScan1
            or StepScanInvoke
            or StepScanResult
            or StepScanStop
            or StepScanProxyFailed
            or StepSettle1
            or StepConnect1
            or StepConnect2
            or StepConnectInvoke
            or StepConnect3
            or StepWaitConnect
            or StepWaitNotify
            or StepPwd1
            or StepPerson1;

    /// <summary>
    /// Successful markers clear the probe; do not report them after relaunch.
    /// Do not clear on <see cref="StepScanStop"/> or <see cref="StepConnect3"/>:
    /// those precede handshake / password / person sync.
    /// </summary>
    public static bool ClearsProbe(string? step) =>
        step is StepInit3 or StepHandshakeOk;

    /// <summary>Patient-facing copy after relaunch. Includes the step code for engineers.</summary>
    public static string PatientMessageForIncompleteStep(string step)
    {
        var detail = step switch
        {
            StepInit1 or StepInit2 =>
                "The app closed while starting the watch SDK (" + step + ").",
            StepScan1 or StepScanInvoke =>
                "The app closed while scanning for the watch through the watch SDK (" + step + ").",
            StepScanResult =>
                "The app closed after the watch SDK found the watch (" + step
                + "), before Connect finished.",
            StepScanStop =>
                "The app closed after stopping the watch SDK scan, before Connect finished ("
                + step + ").",
            StepScanProxyFailed =>
                "The app closed while preparing the watch SDK scan listener (" + step + ").",
            StepSettle1 =>
                "The app closed while waiting for Bluetooth to settle after scan (" + step + ").",
            StepConnect1 =>
                "The app closed while checking the watch link (" + step + ").",
            StepConnect2 or StepConnectInvoke =>
                "The app closed while connecting to the watch through the watch SDK (" + step
                + "). That usually means the native connect call aborted.",
            StepConnect3 =>
                "The app closed after the watch SDK connect call returned, before the watch finished linking ("
                + step + ").",
            StepWaitConnect =>
                "The app closed while waiting for the watch Bluetooth connect callback (" + step + ").",
            StepWaitNotify =>
                "The app closed while waiting for the watch notify callback (" + step + ").",
            StepPwd1 =>
                "The app closed while confirming the watch password (" + step + ").",
            StepPerson1 =>
                "The app closed while syncing person info to the watch (" + step + ").",
            _ => "The app closed during watch Connect (" + step + ")."
        };

        return detail
               + " You can keep using Connect on build 1.8.42. Tell your engineer this step code.";
    }
}
