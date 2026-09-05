using RaphCare.Mobile.Core.Common.Clinics;
using Xunit;

namespace RaphCare.Mobile.Tests;

public sealed class SelectClinicSearchRulesTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyQueryMustNotLoadClinicDirectory(string? input)
    {
        Assert.False(SelectClinicSearchRules.TryNormalizeQuery(input, out var query));
        Assert.Equal(string.Empty, query);
    }

    [Theory]
    [InlineData("Demo", "Demo")]
    [InlineData("  RC-DEMCLN  ", "RC-DEMCLN")]
    [InlineData("Daskana", "Daskana")]
    public void TypedQueryIsNormalizedForDirectorySearch(string input, string expected)
    {
        Assert.True(SelectClinicSearchRules.TryNormalizeQuery(input, out var query));
        Assert.Equal(expected, query);
    }
}
