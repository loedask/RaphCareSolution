using RaphCare.Mobile.Core.Common.Icons;
using RaphCare.Mobile.Core.Common.Navigation;
using Xunit;

namespace RaphCare.Mobile.Tests;

public sealed class MonochromeIconKeysTests
{
    [Fact]
    public void CatalogEntriesAreNotEmojiGlyphs()
    {
        Assert.NotEmpty(MonochromeIconKeys.All);
        foreach (var key in MonochromeIconKeys.All)
        {
            Assert.False(
                MonochromeIconKeys.LooksLikeEmojiGlyph(key),
                $"Icon key '{key}' must stay a monochrome resource name, not an emoji.");
            Assert.EndsWith(".png", key, StringComparison.Ordinal);
            Assert.StartsWith("icon_", key, StringComparison.Ordinal);
        }
    }

    [Theory]
    [InlineData("❤️")]
    [InlineData("🏥")]
    [InlineData("👨‍👩‍👧")]
    public void LooksLikeEmojiGlyphDetectsMulticolorEmoji(string glyph) =>
        Assert.True(MonochromeIconKeys.LooksLikeEmojiGlyph(glyph));

    [Fact]
    public void LooksLikeEmojiGlyphAllowsResourceFileNames() =>
        Assert.False(MonochromeIconKeys.LooksLikeEmojiGlyph(MonochromeIconKeys.Heart));
}

public sealed class AbsoluteShellRouteRulesTests
{
    [Theory]
    [InlineData("HomePage")]
    [InlineData("//HomePage")]
    [InlineData("//RecordsPage?x=1")]
    [InlineData("LandingPage")]
    public void TabRootsAreSafeAbsoluteTargets(string route) =>
        Assert.True(AbsoluteShellRouteRules.IsSafeAbsoluteTarget(route));

    [Theory]
    [InlineData("AccountCreatedPage")]
    [InlineData("//AccountCreatedPage")]
    [InlineData("//SelectClinicPage")]
    [InlineData("PersonalInformationPage")]
    [InlineData("BookAppointmentPage")]
    [InlineData("//BookAppointmentPage")]
    [InlineData("AddInsuranceProfilePage")]
    [InlineData("//BillingPage")]
    [InlineData("TelehealthJoinPage")]
    [InlineData("HealthRecordDetailPage")]
    public void PushOnlyRoutesMustNotUseAbsoluteShellPaths(string route) =>
        Assert.False(
            AbsoluteShellRouteRules.IsSafeAbsoluteTarget(route),
            "Absolute // navigation to push-only routes closes the Android app.");
}
