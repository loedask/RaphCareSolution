using RaphCare.Mobile.Core.Common.Collections;
using Xunit;

namespace RaphCare.Mobile.Tests;

/// <summary>
/// Regression: Android MAUI Picker can close the process when ItemsSource.Clear runs while SelectedItem still points at a row in that list (Book appointment provider picker).
/// </summary>
public sealed class MauiPickerCollectionRulesTests
{
    private sealed class Row
    {
        public required string Name { get; init; }
    }

    [Fact]
    public void IsClearSafeReturnsFalseWhenSelectionStillInList()
    {
        var selected = new Row { Name = "Dr A" };
        var items = new List<Row> { selected, new() { Name = "Dr B" } };

        Assert.False(MauiPickerCollectionRules.IsClearSafe(items, selected));
    }

    [Fact]
    public void IsClearSafeReturnsTrueAfterSelectionCleared()
    {
        var selected = new Row { Name = "Dr A" };
        var items = new List<Row> { selected };

        Assert.True(MauiPickerCollectionRules.IsClearSafe(items, selected: null));
    }

    [Fact]
    public void PickPreferredOrFirstUsesConfiguredProviderWhenPresent()
    {
        var preferred = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var other = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var picked = MauiPickerCollectionRules.PickPreferredOrFirst([other, preferred], preferred);
        Assert.Equal(preferred, picked);
    }

    [Fact]
    public void PickPreferredOrFirstFallsBackToFirstWhenPreferredMissing()
    {
        var first = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var second = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var missing = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var picked = MauiPickerCollectionRules.PickPreferredOrFirst([first, second], missing);
        Assert.Equal(first, picked);
    }
}
