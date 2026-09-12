namespace RaphCare.Mobile.Kernel.Core.Common.Devices;

/// <summary>
/// Pure rules for Veepoo <c>VPOperateManager</c> startup, derived from javap on
/// <c>vpprotocol-2.3.81.15</c>. Keeps Android JNI choices testable without MAUI.
/// </summary>
public static class VeepooSdkInitRules
{
    /// <summary>
    /// Prefer <c>getMangerInstance(Context)</c> (vendor spelling). That path sets
    /// <c>mContext</c>, builds <c>BluetoothClient</c> (<c>vp_dm</c>), and runs the same
    /// setup as a first-time <c>init</c>. <c>getInstance()</c> alone creates the manager
    /// without <c>BluetoothClient</c>; later <c>isDeviceConnected</c> / <c>connectDevice</c>
    /// then NPE on a null client.
    /// </summary>
    public static bool PreferGetManagerInstanceWithContext => true;

    /// <summary>
    /// After a successful obtain, call instance <c>init(ApplicationContext)</c> only when
    /// the static Bluetooth client is still missing (for example a prior bare
    /// <c>getInstance()</c>). When <c>getMangerInstance</c> already ran, <c>init</c> is a no-op.
    /// </summary>
    public static bool ShouldCallInitWhenBluetoothClientMissing(bool bluetoothClientPresent) =>
        !bluetoothClientPresent;

    /// <summary>
    /// javap: the 3-arg <c>connectDevice(mac, IConnectResponse, INotifyResponse)</c> only
    /// forwards to the synchronized 4-arg form with device name <c>"none"</c>. Prefer the
    /// mac+name overload with the advertised watch name (HBand sample style).
    /// </summary>
    public static bool PreferMacPlusNameConnectDeviceOverload(
        bool preferOfficialMacOnlyConnectDeviceOverload) =>
        !preferOfficialMacOnlyConnectDeviceOverload;

    /// <summary>
    /// Do not probe <c>isDeviceConnected</c> / <c>isCurrentDeviceConnected</c> when the
    /// client is missing. Those methods dereference <c>vp_dm</c> with no null check.
    /// </summary>
    public static bool ShouldProbeNativeConnectedLink(bool bluetoothClientPresent) =>
        bluetoothClientPresent;

    /// <summary>
    /// Single-stack probe: discover via Veepoo <c>startScanDevice</c>, then
    /// <c>connectDevice</c> from that same stack. Never starts Plugin.BLE for the session.
    /// </summary>
    public static bool ShouldUseVendorNativeScan(bool useVeepooNativeScanProbe) =>
        useVeepooNativeScanProbe;

    /// <summary>
    /// Accept a vendor-scan hit when it matches the claimed MAC or an E580/E585-style name.
    /// </summary>
    public static bool ShouldAcceptVendorScanResult(
        string? deviceName,
        string? deviceMac,
        string? preferredMac,
        bool nameLooksLikeE580Style) =>
        (preferredMac is not null
         && BluetoothMacNormalizer.EqualsNormalized(deviceMac, preferredMac))
        || nameLooksLikeE580Style;

    /// <summary>
    /// Prefer the untimed <c>startScanDevice(SearchResponse)</c> overload. The timed
    /// <c>startScanDevice(int, …)</c> form may schedule its own stop, which can race with
    /// an explicit <c>stopScanDevice</c> from the coordinator.
    /// </summary>
    public static bool ShouldUseTimedStartScanOverload => false;

    /// <summary>
    /// Java reflection <c>Class.isInstance</c> is always false for primitive parameter types.
    /// Boxed args (for example <c>java.lang.Boolean</c> for <c>boolean</c>) must still match
    /// so <c>Method.invoke</c> can unbox. Huawei 1.9.4: <c>confirmDevicePwd(..., boolean)</c>
    /// never matched when passing <c>Boolean.FALSE</c>.
    /// </summary>
    public static bool IsBoxedCompatibleWithJavaPrimitive(string? parameterTypeName, string? argClassName)
    {
        if (string.IsNullOrWhiteSpace(parameterTypeName) || string.IsNullOrWhiteSpace(argClassName))
            return false;

        return parameterTypeName switch
        {
            "boolean" => argClassName is "boolean" or "java.lang.Boolean",
            "byte" => argClassName is "byte" or "java.lang.Byte",
            "char" => argClassName is "char" or "java.lang.Character",
            "short" => argClassName is "short" or "java.lang.Short",
            "int" => argClassName is "int" or "java.lang.Integer",
            "long" => argClassName is "long" or "java.lang.Long",
            "float" => argClassName is "float" or "java.lang.Float",
            "double" => argClassName is "double" or "java.lang.Double",
            _ => false
        };
    }
}
