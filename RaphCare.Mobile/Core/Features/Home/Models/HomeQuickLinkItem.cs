using System.Windows.Input;

namespace RaphCare.Mobile.Core.Features.Home.Models;

/// <summary>Dashboard card: tap opens a feature via <see cref="NavigateCommand"/>.</summary>
public sealed class HomeQuickLinkItem
{
    public required string Title { get; init; }
    public required string Subtitle { get; init; }
    public required string IconGlyph { get; init; }
    public required ICommand NavigateCommand { get; init; }
}
