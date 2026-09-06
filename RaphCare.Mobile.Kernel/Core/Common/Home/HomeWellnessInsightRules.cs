namespace RaphCare.Mobile.Core.Common.Home;

/// <summary>
/// Maps recent patient mood check-ins (0 great … 3 low) into a Home tip kind.
/// Does not invent "positive week" copy when there are no check-ins.
/// </summary>
public static class HomeWellnessInsightRules
{
    public enum Kind
    {
        None,
        Positive,
        Steady,
        Mixed,
        Low,
    }

    /// <summary>
    /// Classifies mood scores newest-first. Scores outside 0–3 are ignored.
    /// </summary>
    public static Kind Classify(IEnumerable<int> moodScoresNewestFirst, int lookback = 7)
    {
        ArgumentNullException.ThrowIfNull(moodScoresNewestFirst);

        var scores = moodScoresNewestFirst
            .Where(s => s is >= 0 and <= 3)
            .Take(Math.Clamp(lookback, 1, 30))
            .ToList();

        if (scores.Count == 0)
            return Kind.None;

        var latest = scores[0];
        if (latest >= 3)
            return Kind.Low;

        var lowCount = scores.Count(s => s >= 3);
        if (lowCount > 0)
            return Kind.Mixed;

        var average = scores.Average();
        if (average <= 1.0)
            return Kind.Positive;

        if (average <= 2.0)
            return Kind.Steady;

        return Kind.Mixed;
    }
}
