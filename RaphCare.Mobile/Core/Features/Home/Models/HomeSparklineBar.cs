namespace RaphCare.Mobile.Core.Features.Home.Models;

/// <summary>Single bar in a home health metric sparkline.</summary>
public sealed class HomeSparklineBar
{
    public double Height { get; init; }
    public bool IsLatest { get; init; }
}
