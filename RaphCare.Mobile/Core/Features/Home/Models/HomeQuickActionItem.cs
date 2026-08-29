using System.Windows.Input;

namespace RaphCare.Mobile.Core.Features.Home.Models;

/// <summary>3-column quick action tile on the home dashboard (concept <c>Home.tsx</c>).</summary>
public sealed class HomeQuickActionItem
{
    public required string Title { get; init; }

    /// <summary>MAUI image resource (see <c>MonochromeIconKeys</c>), tinted in XAML.</summary>
    public required string IconSource { get; init; }

    public bool IsAccent { get; init; }
    public required ICommand TapCommand { get; init; }
}
