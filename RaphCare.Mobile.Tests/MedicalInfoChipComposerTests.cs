using RaphCare.Mobile.Core.Common.MedicalInfo;
using Xunit;

namespace RaphCare.Mobile.Tests;

public sealed class MedicalInfoChipComposerTests
{
    private static readonly MedicalInfoChipSpec[] AllergyChips =
    [
        new("penicillin", "Penicillin", false),
        new("sulfa", "Sulfa", false),
        new("latex", "Latex", false),
        new("peanuts", "Peanuts", false),
        new("shellfish", "Shellfish", false),
        new("none", "No known allergies", true),
    ];

    private static readonly string[] AllergyNoneAliases =
    [
        "NKDA",
        "no known allergies",
    ];

    [Fact]
    public void ComposeJoinsSelectedChipsAndOtherNotes()
    {
        var stored = MedicalInfoChipComposer.Compose(
            AllergyChips,
            ["penicillin", "latex"],
            "bee stings");

        Assert.Equal("Penicillin, Latex, bee stings", stored);
    }

    [Fact]
    public void ComposeExclusiveChipReplacesOtherChips()
    {
        var stored = MedicalInfoChipComposer.Compose(
            AllergyChips,
            ["penicillin", "none"],
            null);

        Assert.Equal("No known allergies", stored);
    }

    [Fact]
    public void ParseRestoresChipsAndLeavesUnknownTextInOther()
    {
        var (selected, other) = MedicalInfoChipComposer.Parse(
            "Penicillin, bee stings, Latex",
            AllergyChips);

        Assert.Contains("penicillin", selected);
        Assert.Contains("latex", selected);
        Assert.Equal("bee stings", other);
    }

    [Fact]
    public void ParseMapsNkdaAliasToExclusiveChip()
    {
        var (selected, other) = MedicalInfoChipComposer.Parse(
            "NKDA",
            AllergyChips,
            AllergyNoneAliases);

        Assert.Contains("none", selected);
        Assert.Equal(string.Empty, other);
    }

    [Fact]
    public void ParseExclusiveClearsConflictingChipIds()
    {
        var (selected, other) = MedicalInfoChipComposer.Parse(
            "No known allergies, Penicillin",
            AllergyChips);

        Assert.Contains("none", selected);
        Assert.DoesNotContain("penicillin", selected);
        Assert.Equal(string.Empty, other);
    }
}
