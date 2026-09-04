using Xunit;
using RaphCare.Portal.Helpers;

namespace RaphCare.Portal.Tests.Helpers;

public sealed class HospitalDeckReturnUrlTests
{
    private static readonly Guid ClinicId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    [Fact]
    public void ResolveUsesSafeSameClinicReturnWhenValid()
    {
        var inbound = $"/admin/hospitals/{ClinicId}/inpatient?tab=admissions";
        var result = HospitalDeckReturnUrl.Resolve(
            ClinicId,
            Uri.EscapeDataString(inbound),
            HospitalDeckReturnUrl.ForClinicTab(ClinicId, "patients"));

        Assert.Equal(inbound, result);
    }

    [Fact]
    public void ResolveFallsBackWhenReturnPointsAtAnotherClinic()
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
    public void ResolveFallsBackWhenReturnIsAbsoluteUrl()
    {
        var fallback = HospitalDeckReturnUrl.ForClinicTab(ClinicId, "patients");
        var result = HospitalDeckReturnUrl.Resolve(
            ClinicId,
            "https://evil.example/admin/hospitals/" + ClinicId,
            fallback);

        Assert.Equal(fallback, result);
    }

    [Fact]
    public void PatientHrefEmbedsEscapedReturn()
    {
        var href = HospitalDeckReturnUrl.PatientHref(
            ClinicId,
            Guid.Parse("99999999-9999-9999-9999-999999999999"),
            HospitalDeckReturnUrl.ForClinicPage(ClinicId, "inpatient", "admissions"));

        Assert.Contains("return=", href, StringComparison.Ordinal);
        Assert.Contains("inpatient", href, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("casualty")]
    [InlineData("theatre")]
    [InlineData("inpatient")]
    [InlineData("collection")]
    [InlineData("referrals")]
    [InlineData("roster")]
    public void LinkedSectionBackReturnKeepsSectionTabOnHospitalProfile(string section)
    {
        var back = HospitalDeckReturnUrl.ForLinkedSectionReturn(ClinicId, section);

        Assert.Equal($"/admin/hospitals/{ClinicId}?tab={section}", back);
        Assert.True(HospitalDeckReturnUrl.IsLinkedSectionTab(section));
    }

    [Fact]
    public void OverviewIsNotALinkedSectionTab()
    {
        Assert.False(HospitalDeckReturnUrl.IsLinkedSectionTab("overview"));
        Assert.False(HospitalDeckReturnUrl.IsLinkedSectionTab(null));
    }
}
