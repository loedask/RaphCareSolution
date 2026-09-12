using RaphCare.Domain.Organization;
using Xunit;

namespace RaphCare.Application.Tests.Features.Organization;

public sealed class ClinicCommercialPlanFeaturesTests
{
    [Theory]
    [InlineData(ClinicCommercialPlan.Practice)]
    [InlineData(ClinicCommercialPlan.Clinic)]
    [InlineData("practice")]
    [InlineData("clinic")]
    public void PracticeOrClinicPlanMustNotAllowCollectionOrInpatient(string plan)
    {
        Assert.False(ClinicCommercialPlanFeatures.HasInpatient(plan));
        Assert.False(ClinicCommercialPlanFeatures.HasCollection(plan));
        Assert.False(ClinicCommercialPlanFeatures.HasCasualty(plan));
        Assert.False(ClinicCommercialPlanFeatures.HasTheatre(plan));
        Assert.True(ClinicCommercialPlanFeatures.HasConsultWaiting(plan));
    }

    [Theory]
    [InlineData(ClinicCommercialPlan.Hospital)]
    [InlineData(ClinicCommercialPlan.Network)]
    [InlineData("hospital")]
    [InlineData("network")]
    public void HospitalOrNetworkPlanAllowsStayAndFrontOfHouse(string plan)
    {
        Assert.True(ClinicCommercialPlanFeatures.HasInpatient(plan));
        Assert.True(ClinicCommercialPlanFeatures.HasCollection(plan));
        Assert.True(ClinicCommercialPlanFeatures.HasCasualty(plan));
        Assert.True(ClinicCommercialPlanFeatures.HasTheatre(plan));
        Assert.True(ClinicCommercialPlanFeatures.HasConsultWaiting(plan));
    }

    [Fact]
    public void EmptyPlanDefaultsLikeClinic()
    {
        Assert.False(ClinicCommercialPlanFeatures.HasInpatient(null));
        Assert.False(ClinicCommercialPlanFeatures.HasCollection(""));
        Assert.True(ClinicCommercialPlanFeatures.HasConsultWaiting(null));
        Assert.Equal(ClinicCommercialPlan.Clinic, ClinicCommercialPlan.Normalize("not-a-plan"));
    }
}
