using System.Collections.ObjectModel;
using Microsoft.Maui.Graphics;

namespace RaphCare.Mobile.Core.Features.Home.Models;

/// <summary>Health summary card with value and synced timestamp (live patient readings).</summary>
public sealed class HomeHealthMetricItem
{
    public required string Label { get; init; }
    public required string Value { get; init; }
    public required string Unit { get; init; }
    public required string Status { get; init; }
    public required string TrendLabel { get; init; }
    public required string TrendGlyph { get; init; }

    /// <summary>MAUI image resource (see <c>MonochromeIconKeys</c>).</summary>
    public required string IconSource { get; init; }

    /// <summary>Soft tint behind the icon (rose / sky / mint family).</summary>
    public required Color IconWellColor { get; init; }

    public required ObservableCollection<HomeSparklineBar> Sparkline { get; init; }
}
