namespace RaphCare.Mobile.Core.Features.Devices.Models;

/// <summary>Last parsed vitals or raw notify payload from a connected wearable.</summary>
public sealed class WearableVitalsSnapshot
{
    public DateTimeOffset At { get; init; } = DateTimeOffset.UtcNow;
    public int? HeartRateBpm { get; init; }
    public string CharacteristicUuid { get; init; } = string.Empty;
    public string RawHex { get; init; } = string.Empty;
}
