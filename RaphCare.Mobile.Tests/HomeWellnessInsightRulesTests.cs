using RaphCare.Mobile.Core.Common.Home;
using Xunit;

namespace RaphCare.Mobile.Tests;

public sealed class HomeWellnessInsightRulesTests
{
    [Fact]
    public void ClassifyReturnsNoneWhenNoScores() =>
        Assert.Equal(HomeWellnessInsightRules.Kind.None, HomeWellnessInsightRules.Classify([]));

    [Fact]
    public void ClassifyReturnsLowWhenLatestIsLow() =>
        Assert.Equal(HomeWellnessInsightRules.Kind.Low, HomeWellnessInsightRules.Classify([3, 0, 0]));

    [Fact]
    public void ClassifyReturnsPositiveWhenRecentScoresAreGreatOrGood() =>
        Assert.Equal(HomeWellnessInsightRules.Kind.Positive, HomeWellnessInsightRules.Classify([0, 1, 0, 1]));

    [Fact]
    public void ClassifyReturnsMixedWhenOlderLowExistsButLatestIsOk() =>
        Assert.Equal(HomeWellnessInsightRules.Kind.Mixed, HomeWellnessInsightRules.Classify([1, 3, 0]));

    [Fact]
    public void ClassifyReturnsSteadyForMostlyOkayScores() =>
        Assert.Equal(HomeWellnessInsightRules.Kind.Steady, HomeWellnessInsightRules.Classify([2, 2, 1, 2]));

    [Fact]
    public void ClassifyIgnoresOutOfRangeScores() =>
        Assert.Equal(HomeWellnessInsightRules.Kind.None, HomeWellnessInsightRules.Classify([9, -1]));
}
