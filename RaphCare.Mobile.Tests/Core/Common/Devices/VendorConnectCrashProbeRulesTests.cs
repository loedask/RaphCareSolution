using RaphCare.Mobile.Kernel.Core.Common.Devices;
using Xunit;

namespace RaphCare.Mobile.Tests.Core.Common.Devices;

public sealed class VendorConnectCrashProbeRulesTests
{
    [Theory]
    [InlineData(VendorConnectCrashProbeRules.StepSettle1)]
    [InlineData(VendorConnectCrashProbeRules.StepConnect2)]
    [InlineData(VendorConnectCrashProbeRules.StepConnectInvoke)]
    [InlineData(VendorConnectCrashProbeRules.StepConnect3)]
    [InlineData(VendorConnectCrashProbeRules.StepWaitConnect)]
    [InlineData(VendorConnectCrashProbeRules.StepWaitNotify)]
    [InlineData(VendorConnectCrashProbeRules.StepPwd1)]
    [InlineData(VendorConnectCrashProbeRules.StepPerson1)]
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
    [InlineData(VendorConnectCrashProbeRules.StepHandshakeOk)]
    [InlineData(VendorConnectCrashProbeRules.StepInit3)]
    public void SuccessfulStepsClearProbeAndAreNotReported(string step)
    {
        Assert.True(VendorConnectCrashProbeRules.ClearsProbe(step));
        Assert.False(VendorConnectCrashProbeRules.ShouldReportIncompleteStep(step));
    }

    [Fact]
    public void Connect3MustNotClearProbeRegressionFromPhoneTrail1849()
    {
        // Phone trail reached CONNECT-3 then relaunched without HANDSHAKE-OK. Clearing on
        // CONNECT-3 hid the incomplete step after relaunch.
        Assert.False(VendorConnectCrashProbeRules.ClearsProbe(VendorConnectCrashProbeRules.StepConnect3));
        Assert.True(VendorConnectCrashProbeRules.ShouldReportIncompleteStep(
            VendorConnectCrashProbeRules.StepConnect3));
        var message = VendorConnectCrashProbeRules.PatientMessageForIncompleteStep(
            VendorConnectCrashProbeRules.StepConnect3);
        Assert.Contains("connect call returned", message, StringComparison.OrdinalIgnoreCase);
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
