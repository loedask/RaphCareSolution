using RaphCare.Mobile.Kernel.Core.Common.Devices;
using Xunit;

namespace RaphCare.Mobile.Tests.Core.Common.Devices;

public sealed class VendorConnectCrashProbeRulesTests
{
    [Theory]
    [InlineData(VendorConnectCrashProbeRules.StepConnect2)]
    [InlineData(VendorConnectCrashProbeRules.StepConnectInvoke)]
    [InlineData(VendorConnectCrashProbeRules.StepInit1)]
    [InlineData(VendorConnectCrashProbeRules.StepConnect1)]
    [InlineData(VendorConnectCrashProbeRules.StepScan1)]
    [InlineData(VendorConnectCrashProbeRules.StepScanInvoke)]
    [InlineData(VendorConnectCrashProbeRules.StepScanResult)]
    [InlineData(VendorConnectCrashProbeRules.StepScanStop)]
    [InlineData(VendorConnectCrashProbeRules.StepScanProxyFailed)]
    public void IncompleteConnectStepsMustBeReportedAfterRelaunch(string step)
    {
        Assert.True(VendorConnectCrashProbeRules.ShouldReportIncompleteStep(step));
        Assert.False(VendorConnectCrashProbeRules.ClearsProbe(step));
        var message = VendorConnectCrashProbeRules.PatientMessageForIncompleteStep(step);
        Assert.Contains(step, message, StringComparison.Ordinal);
        Assert.Contains("1.8.42", message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(VendorConnectCrashProbeRules.StepConnect3)]
    [InlineData(VendorConnectCrashProbeRules.StepHandshakeOk)]
    [InlineData(VendorConnectCrashProbeRules.StepInit3)]
    public void SuccessfulStepsClearProbeAndAreNotReported(string step)
    {
        Assert.True(VendorConnectCrashProbeRules.ClearsProbe(step));
        Assert.False(VendorConnectCrashProbeRules.ShouldReportIncompleteStep(step));
    }

    [Fact]
    public void ScanStopMessageMustDistinguishDiedAfterScanBeforeConnect()
    {
        // Regression: 1.8.47 ClearedProbe on SCAN-STOP, so a crash before connectDevice
        // left no relaunch message. SCAN-STOP must report as incomplete and say so clearly.
        var scanStop = VendorConnectCrashProbeRules.PatientMessageForIncompleteStep(
            VendorConnectCrashProbeRules.StepScanStop);
        var connectInvoke = VendorConnectCrashProbeRules.PatientMessageForIncompleteStep(
            VendorConnectCrashProbeRules.StepConnectInvoke);
        Assert.Contains("stopping the watch SDK scan", scanStop, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("before Connect finished", scanStop, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stopping the watch SDK scan", connectInvoke, StringComparison.OrdinalIgnoreCase);
    }
}
