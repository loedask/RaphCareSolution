using System.Collections.ObjectModel;

namespace RaphCare.Mobile.Core.Features.Home.Models;

/// <summary>Health summary card with value, trend, and sparkline (concept mock data).</summary>
public sealed class HomeHealthMetricItem
{
    public required string Label { get; init; }
    public required string Value { get; init; }
    public required string Unit { get; init; }
    public required string Status { get; init; }
    public required string TrendLabel { get; init; }
    public required string TrendGlyph { get; init; }
    public required string IconGlyph { get; init; }
    public required ObservableCollection<HomeSparklineBar> Sparkline { get; init; }
}
