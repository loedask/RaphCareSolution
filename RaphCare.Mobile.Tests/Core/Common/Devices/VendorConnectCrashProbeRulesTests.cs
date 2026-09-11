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
    public void IncompleteConnectStepsMustBeReportedAfterRelaunch(string step)
    {
        Assert.True(VendorConnectCrashProbeRules.ShouldReportIncompleteStep(step));
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
}
