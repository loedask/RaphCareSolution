namespace RaphCare.Mobile.Core.Features.Devices.HBand;

/// <summary>One advertisement from Veepoo <c>startScanDevice</c> (single-stack probe).</summary>
public sealed class VendorScanDeviceFoundEventArgs : EventArgs
{
    public required string MacAddress { get; init; }
    public string? Name { get; init; }
    public int Rssi { get; init; }
}
