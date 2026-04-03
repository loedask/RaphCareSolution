namespace RaphCare.Mobile.Core.Features.Devices.Models;

/// <summary>Last parsed vitals or raw notify payload from a connected wearable.</summary>
public sealed class WearableVitalsSnapshot
{
    public DateTimeOffset At { get; init; } = DateTimeOffset.UtcNow;
    public int? HeartRateBpm { get; init; }
    /// <summary>SpO₂ percentage when a standard PLX notify (0x2A60 / 0x2A5F) is decoded.</summary>
    public decimal? SpO2Percent { get; init; }
    /// <summary>Pulse rate carried in PLX measurement (may match <see cref="HeartRateBpm"/> from HR service).</summary>
    public int? SpO2PulseBpm { get; init; }
    public string CharacteristicUuid { get; init; } = string.Empty;
    public string RawHex { get; init; } = string.Empty;
}
