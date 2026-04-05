namespace RaphCare.Domain.Devices;

/// <summary>Normalized <see cref="DeviceEmergencyEvent.EventType"/> values for 4G / OEM webhooks.</summary>
public static class StandaloneEmergencyEventTypes
{
    public const string Sos = "SOS";
    public const string Fall = "Fall";
    public const string LocationPing = "LocationPing";
    public const string LowBattery = "LowBattery";
}
