using RaphCare.Mobile.Core.Common.Settings;
using Xunit;

namespace RaphCare.Mobile.Tests;

public sealed class EmergencyContactPhonePickerRulesTests
{
    [Fact]
    public void MapFromPhoneContactUsesDisplayNameAndFirstPhone()
    {
        var mapped = EmergencyContactPhonePickerRules.MapFromPhoneContact(
            "  Ada Lovelace  ",
            ["  +27 82 000 0001  ", "+27 82 000 0002"]);

        Assert.Equal("Ada Lovelace", mapped.Name);
        Assert.Equal("+27 82 000 0001", mapped.Phone);
        Assert.True(mapped.HasPhone);
    }

    [Fact]
    public void MapFromPhoneContactReportsNoPhoneWhenListEmptyOrBlank()
    {
        var empty = EmergencyContactPhonePickerRules.MapFromPhoneContact("Sam", []);
        Assert.Equal("Sam", empty.Name);
        Assert.Equal(string.Empty, empty.Phone);
        Assert.False(empty.HasPhone);

        var blanks = EmergencyContactPhonePickerRules.MapFromPhoneContact("Sam", ["  ", null, ""]);
        Assert.False(blanks.HasPhone);
        Assert.Equal(string.Empty, blanks.Phone);
    }

    [Fact]
    public void MapFromPhoneContactAllowsEmptyNameWhenPhonePresent()
    {
        var mapped = EmergencyContactPhonePickerRules.MapFromPhoneContact("   ", ["555-0100"]);
        Assert.Equal(string.Empty, mapped.Name);
        Assert.Equal("555-0100", mapped.Phone);
        Assert.True(mapped.HasPhone);
    }

    [Fact]
    public void DistinctPhoneChoicesSkipsBlanksAndDuplicates()
    {
        var choices = EmergencyContactPhonePickerRules.DistinctPhoneChoices(
            [" 111 ", null, "111", "222", "  ", "222 "]);

        Assert.Equal(["111", "222"], choices);
    }
}
