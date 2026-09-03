using Xunit;
using RaphCare.Portal.Helpers;

namespace RaphCare.Portal.Tests.Helpers;

public sealed class HospitalDeckReturnUrlTests
{
    private static readonly Guid ClinicId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    [Fact]
    public void Resolve_UsesSafeSameClinicReturn_WhenValid()
    {
        var inbound = $"/admin/hospitals/{ClinicId}/inpatient?tab=admissions";
        var result = HospitalDeckReturnUrl.Resolve(
            ClinicId,
            Uri.EscapeDataString(inbound),
            HospitalDeckReturnUrl.ForClinicTab(ClinicId, "patients"));

        Assert.Equal(inbound, result);
    }

    [Fact]
    public void Resolve_FallsBack_WhenReturnPointsAtAnotherClinic()
    {
        var other = Guid.Parse("11111111-2222-3333-4444-555555555555");
        var fallback = HospitalDeckReturnUrl.ForClinicTab(ClinicId, "patients");
        var result = HospitalDeckReturnUrl.Resolve(
            ClinicId,
            $"/admin/hospitals/{other}?tab=patients",
            fallback);

        Assert.Equal(fallback, result);
    }

    [Fact]
    public void Resolve_FallsBack_WhenReturnIsAbsoluteUrl()
    {
        var fallback = HospitalDeckReturnUrl.ForClinicTab(ClinicId, "patients");
        var result = HospitalDeckReturnUrl.Resolve(
            ClinicId,
            "https://evil.example/admin/hospitals/" + ClinicId,
            fallback);

        Assert.Equal(fallback, result);
    }

    [Fact]
    public void PatientHref_EmbedsEscapedReturn()
    {
        var href = HospitalDeckReturnUrl.PatientHref(
            ClinicId,
            Guid.Parse("99999999-9999-9999-9999-999999999999"),
            HospitalDeckReturnUrl.ForClinicPage(ClinicId, "inpatient", "admissions"));

        Assert.Contains("return=", href, StringComparison.Ordinal);
        Assert.Contains("inpatient", href, StringComparison.Ordinal);
    }
}
