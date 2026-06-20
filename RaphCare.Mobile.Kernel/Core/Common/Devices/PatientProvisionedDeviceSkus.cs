namespace RaphCare.Mobile.Kernel.Core.Common.Devices;

/// <summary>
/// Stable SKU identifiers for RaphCare-provided patient hardware (see docs/13_Patient_Device_Packages_and_Fleet.md).
/// Use these in future API payloads and analytics instead of ad-hoc strings.
/// </summary>
public static class PatientProvisionedDeviceSkus
{
    /// <summary>4G standalone emergency tracker (Y6 Pro). Not the same BLE vertical as E580/E585.</summary>
    public const string Y6Pro = "Y6Pro";

    /// <summary>BLE health monitoring watch (HBand-class).</summary>
    public const string E585 = "E585";

    /// <summary>Alternative BLE health bracelet; same SDK family as E585.</summary>
    public const string E580 = "E580";
}
