namespace RaphCare.Mobile.Kernel.Core.Common.Devices;

/// <summary>
/// BLE session rules for the patient Devices page.
/// The coordinator is a process singleton; leaving the page must keep an active GATT link
/// so returning can show Connected and resume vitals without pairing again.
/// </summary>
public static class DevicesBleSessionPolicy
{
    /// <summary>
    /// When true, <c>OnDisappearing</c> would call Disconnect. Product rule: leave the link up.
    /// </summary>
    public static bool DisconnectWhenLeavingDevicesPage => false;

    /// <summary>
    /// Stop scan must stay available while the Devices UI shows an in-flight scan.
    /// Do not wait for adapter <c>IsScanning</c> alone: that flag can lag behind the UI, so the
    /// button never enables and Stop appears broken.
    /// </summary>
    public static bool CanStopScan(bool isScanningUi, bool adapterIsScanning) =>
        isScanningUi || adapterIsScanning;

    /// <summary>
    /// When true, Connect tries the Veepoo/HBand SDK before Plugin.BLE.
    /// Keep off for patient builds: the vendor stack often shows a brief Android toast
    /// ("This feature is not supported") when it probes BLE advertising, then we fall back to GATT anyway.
    /// </summary>
    public static bool TryVendorSdkOnConnect => false;
}
