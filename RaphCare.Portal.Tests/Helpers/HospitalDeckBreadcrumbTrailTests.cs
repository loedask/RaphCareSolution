using System.Globalization;
using Xunit;
using RaphCare.Portal.Helpers;
using RaphCare.Portal.Resources.Strings;

namespace RaphCare.Portal.Tests.Helpers;

public sealed class HospitalDeckBreadcrumbTrailTests
{
    private static readonly Guid ClinicId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    [Fact]
    public void BuildWithSectionMarksCurrentAndLinksHospital()
    {
        var crumbs = HospitalDeckBreadcrumbTrail.Build(ClinicId, "Demo Clinic", current: "Casualty");

        Assert.Equal(3, crumbs.Count);
        Assert.Equal("/admin/hospitals", crumbs[0].Href);
        Assert.Equal($"/admin/hospitals/{ClinicId}", crumbs[1].Href);
        Assert.Equal("Demo Clinic", crumbs[1].Label);
        Assert.Equal("Casualty", crumbs[2].Label);
        Assert.Null(crumbs[2].Href);
    }

    [Fact]
    public void BuildWithMidAndLeafKeepsPatientsStepLinked()
    {
        var patientsHref = HospitalDeckReturnUrl.ForClinicTab(ClinicId, "patients");
        var crumbs = HospitalDeckBreadcrumbTrail.Build(
            ClinicId,
            "Demo Clinic",
            current: "Ada Lovelace",
            midLabel: "Patients",
            midHref: patientsHref);

        Assert.Equal(4, crumbs.Count);
        Assert.Equal(patientsHref, crumbs[2].Href);
        Assert.Equal("Patients", crumbs[2].Label);
        Assert.Equal("Ada Lovelace", crumbs[3].Label);
        Assert.Null(crumbs[3].Href);
    }

    [Fact]
    public void BuildWithoutSectionMakesHospitalCurrent()
    {
        var crumbs = HospitalDeckBreadcrumbTrail.Build(ClinicId, "Demo Clinic");

        Assert.Equal(2, crumbs.Count);
        Assert.Equal(
            AppResources.T("AdminLayout_AllHospitals", CultureInfo.CurrentUICulture),
            crumbs[0].Label);
        Assert.Equal("Demo Clinic", crumbs[1].Label);
        Assert.Null(crumbs[1].Href);
    }
}
