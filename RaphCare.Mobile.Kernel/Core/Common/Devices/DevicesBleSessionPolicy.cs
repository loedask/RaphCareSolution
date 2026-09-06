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
    /// Keep off for patient Connect: the vendor stack often shows a brief Android toast
    /// ("This feature is not supported") when it probes BLE advertising.
    /// Live HR/SpO₂ use <c>MeasureLiveVitalsAsync</c> on the Watch readings page instead.
    /// </summary>
    public static bool TryVendorSdkOnConnect => false;

    /// <summary>How long Measure waits for the first heart rate or oxygen sample after handshake.</summary>
    public static TimeSpan LiveMeasureTimeout { get; } = TimeSpan.FromSeconds(40);

    /// <summary>
    /// Vendor connect + notify must finish within this window or Measure aborts.
    /// Includes room for a few REQUEST_CANCELED (-2) retries after GATT handoff.
    /// </summary>
    public static TimeSpan VendorHandshakeTimeout { get; } = TimeSpan.FromSeconds(45);

    /// <summary>Pause after dropping Plugin.BLE so the vendor stack can reclaim the radio.</summary>
    public static TimeSpan PostGattDisconnectSettle { get; } = TimeSpan.FromMilliseconds(2500);

    /// <summary>How many times Measure retries vendor connect after Inuker REQUEST_CANCELED (-2).</summary>
    public static int VendorConnectMaxAttempts { get; } = 3;

    /// <summary>Total Measure budget (handshake settle + sample wait).</summary>
    public static TimeSpan LiveMeasureOverallTimeout { get; } =
        VendorHandshakeTimeout + LiveMeasureTimeout + PostGattDisconnectSettle + TimeSpan.FromSeconds(15);
}
