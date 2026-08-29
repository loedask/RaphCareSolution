using System.Windows.Input;

namespace RaphCare.Mobile.Core.Features.Home.Models;

/// <summary>Connected device row on the home dashboard.</summary>
public sealed class HomeConnectedDeviceItem
{
    public required string Name { get; init; }
    public required string Value { get; init; }
    public required string Unit { get; init; }
    public required string Synced { get; init; }

    /// <summary>MAUI image resource (see <c>MonochromeIconKeys</c>), tinted in XAML.</summary>
    public required string IconSource { get; init; }

    public required ICommand TapCommand { get; init; }
}
