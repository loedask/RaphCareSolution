using RaphCare.Mobile.Core.Common.Clinics;
using Xunit;

namespace RaphCare.Mobile.Tests;

public sealed class LinkedClinicAccessLabelRulesTests
{
    [Theory]
    [InlineData("Registered", "SelectClinicAccessRegistered")]
    [InlineData("ManualGrant", "SelectClinicAccessManual")]
    [InlineData("EncounterBased", "SelectClinicAccessEncounter")]
    [InlineData("InsuranceLinked", "SelectClinicAccessInsurance")]
    [InlineData("CareHistory", "SelectClinicAccessCareHistory")]
    [InlineData(null, "SelectClinicAccessCareHistory")]
    [InlineData(" ", "SelectClinicAccessCareHistory")]
    public void ResourceKeyMapsKnownKinds(string? kind, string expected) =>
        Assert.Equal(expected, LinkedClinicAccessLabelRules.ResourceKey(kind));
}
