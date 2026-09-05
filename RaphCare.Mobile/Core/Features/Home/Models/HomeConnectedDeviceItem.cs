using System.Windows.Input;
using Microsoft.Maui.Graphics;

namespace RaphCare.Mobile.Core.Features.Home.Models;

/// <summary>Connected device row on the home dashboard.</summary>
public sealed class HomeConnectedDeviceItem
{
    public required string Name { get; init; }
    public required string Value { get; init; }
    public required string Unit { get; init; }
    public required string Synced { get; init; }

    /// <summary>MAUI image resource (see <c>MonochromeIconKeys</c>).</summary>
    public required string IconSource { get; init; }

    /// <summary>Soft tint behind the icon.</summary>
    public required Color IconWellColor { get; init; }

    public required ICommand TapCommand { get; init; }
}
