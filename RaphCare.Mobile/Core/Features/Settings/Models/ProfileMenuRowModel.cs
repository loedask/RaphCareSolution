using System.Windows.Input;

namespace RaphCare.Mobile.Core.Features.Settings.Models;

/// <summary>One tappable row on the profile hub (concept Profile.tsx sections).</summary>
public sealed class ProfileMenuRowModel
{
    public required string Title { get; init; }
    public string? Subtitle { get; init; }

    /// <summary>MAUI image resource (see <c>MonochromeIconKeys</c>), tinted in XAML.</summary>
    public required string IconSource { get; init; }

    public bool ShowSeparator { get; init; } = true;
    public required ICommand TapCommand { get; init; }
}
