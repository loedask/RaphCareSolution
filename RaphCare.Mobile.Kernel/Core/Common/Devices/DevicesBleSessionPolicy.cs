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
    /// Closing the app drops in-memory GATT and vendor state. Opening Devices (or Watch readings)
    /// should reconnect the claimed Bluetooth address without asking the patient to Scan first.
    /// </summary>
    public static bool ReconnectClaimedWatchWhenDevicesAppears => true;

    /// <summary>
    /// Opening Devices must not show the Nearby-devices dialog then immediately call Veepoo
    /// <c>connectDevice</c>. That grant→JNI sequence force-closes the app on some phones.
    /// Check permission on appear; Request only from explicit Scan or Connect.
    /// </summary>
    public static bool ShouldRequestBluetoothPermissionOnDevicesAppear => false;

    /// <summary>
    /// After the user grants Nearby devices, wait before Scan/Connect JNI so the Activity
    /// result finishes and the main looper is idle.
    /// </summary>
    public static TimeSpan PostPermissionGrantSettle { get; } = TimeSpan.FromMilliseconds(800);

    /// <summary>
    /// Closing the app drops in-memory GATT and vendor state. Opening Devices (or Watch readings)
    /// should reconnect the claimed Bluetooth address without asking the patient to Scan first.
    /// Also re-run when UI shows Connected but the exclusive Measure session is missing.
    /// </summary>
    public static bool ShouldReconnectClaimedWatchOnAppear(
        bool hasLockedBluetoothMac,
        bool hasConnectedDeviceId,
        bool liveMeasureSessionReady) =>
        ReconnectClaimedWatchWhenDevicesAppears
        && hasLockedBluetoothMac
        && (!hasConnectedDeviceId || !liveMeasureSessionReady);

    /// <summary>
    /// Auto-reconnect on appear only when Nearby devices is already granted.
    /// </summary>
    public static bool ShouldReconnectClaimedWatchOnAppearWithPermission(
        bool nearbyDevicesPermissionGranted,
        bool hasLockedBluetoothMac,
        bool hasConnectedDeviceId,
        bool liveMeasureSessionReady) =>
        nearbyDevicesPermissionGranted
        && ShouldReconnectClaimedWatchOnAppear(
            hasLockedBluetoothMac,
            hasConnectedDeviceId,
            liveMeasureSessionReady);

    /// <summary>
    /// Exclusive Measure needs the Veepoo session, not Plugin.BLE Connected alone.
    /// </summary>
    public static bool IsExclusiveLiveMeasureSessionReady(
        bool preferExclusiveVendorSession,
        bool vendorSdkAvailable,
        bool coordinatorUsingVendorSession,
        bool bridgeSessionReady) =>
        !preferExclusiveVendorSession
        || !vendorSdkAvailable
        || coordinatorUsingVendorSession
        || bridgeSessionReady;

    /// <summary>
    /// Mid-Measure handshake is safe only when Plugin.BLE is not holding GATT.
    /// Stealing the radio from an active GATT link force-closed the app.
    /// </summary>
    public static bool AllowVendorConnectDuringMeasureWhenNoGatt => true;

    /// <summary>
    /// When the watch SDK is not on the phone, reconnect by a short Scan then GATT Connect.
    /// Do not do that while the vendor session is the Connect path (dual-stack crashed the app).
    /// </summary>
    public static bool ShouldUsePluginBleReconnectWhenVendorUnavailable(bool vendorSdkAvailable) =>
        !vendorSdkAvailable;

    /// <summary>How long auto-reconnect may scan for the claimed MAC before giving up.</summary>
    public static TimeSpan ClaimedWatchReconnectScanTimeout { get; } = TimeSpan.FromSeconds(12);

    /// <summary>
    /// Tapping Connect (or auto-reconnect) must not call vendor disconnect then connect
    /// on the same MAC. That teardown after the app was closed force-closed Android.
    /// </summary>
    public static bool ShouldReuseExistingVendorSession(
        bool vendorSessionReady,
        string? sessionMac,
        string? targetMac) =>
        vendorSessionReady && BluetoothMacNormalizer.EqualsNormalized(sessionMac, targetMac);

    /// <summary>
    /// Only wait for the radio after actually dropping a Plugin.BLE GATT link.
    /// A scan-only peripheral is already disconnected; waiting still delayed Connect.
    /// </summary>
    public static bool ShouldWaitAfterReleasingPluginBle(bool pluginBleWasConnected) =>
        pluginBleWasConnected;

    /// <summary>
    /// Stop scan must stay available while the Devices UI shows an in-flight scan.
    /// Do not wait for adapter <c>IsScanning</c> alone: that flag can lag behind the UI, so the
    /// button never enables and Stop appears broken.
    /// </summary>
    public static bool CanStopScan(bool isScanningUi, bool adapterIsScanning) =>
        isScanningUi || adapterIsScanning;

    /// <summary>
    /// When true and the HBand SDK is present with a MAC, Connect uses Veepoo only
    /// (handshake, no live detect yet). No Plugin.BLE GATT fallback for that attempt.
    /// Off for daily builds: stable Connect is Plugin.BLE (<c>1.8.42</c>). Do not reuse this
    /// flag for the single-stack scan probe; use <see cref="UseVeepooNativeScanProbe"/>.
    /// </summary>
    public static bool PreferExclusiveVendorSession => false;

    /// <summary>
    /// Engineer-only probe: Veepoo <c>startScanDevice</c> then <c>connectDevice</c> in one
    /// stack. Never starts Plugin.BLE scan/connect for that session. Separate from
    /// <see cref="PreferExclusiveVendorSession"/> so the failed hybrid path stays off.
    /// Probe <c>1.8.46</c>: on for diagnostic APK only.
    /// </summary>
    public static bool UseVeepooNativeScanProbe => true;

    /// <summary>How long the vendor-native scan probe waits for a matching advertisement.</summary>
    public static TimeSpan VendorNativeScanTimeout { get; } = TimeSpan.FromSeconds(20);

    /// <summary>
    /// When true, call the 3-arg wiki form first. javap shows that overload only forwards to
    /// the synchronized 4-arg <c>connectDevice</c> with device name <c>"none"</c>, so it is
    /// not a safer alternate path. Keep false: use mac+name with the advertised watch name
    /// (HBand sample style) when exclusive Connect is re-enabled.
    /// </summary>
    public static bool PreferOfficialMacOnlyConnectDeviceOverload => false;

    /// <summary>
    /// After Plugin.BLE <c>StopScan</c> or Veepoo <c>stopScanDevice</c>, wait before
    /// <c>connectDevice</c> so the adapter can settle. Immediate stop-then-connect force-closed
    /// some phones at WAIT-CONNECT (probe trail 1.8.50).
    /// </summary>
    public static TimeSpan PostScanStopSettleBeforeVendorConnect { get; } =
        TimeSpan.FromMilliseconds(1500);

    /// <summary>
    /// After a native Connect abort, Devices appear must show the breadcrumb and must not
    /// auto-reconnect (that re-enters connectDevice and can crash again before the user reads it).
    /// </summary>
    public static bool ShouldSkipClaimedWatchReconnectAfterCrashProbe(
        bool hasIncompleteCrashProbeStep) =>
        hasIncompleteCrashProbeStep;

    /// <summary>
    /// Opening Devices should warm-load <c>VPOperateManager</c> + Inuker <c>BluetoothClient</c>
    /// without calling <c>connectDevice</c>. Surfaces INIT log markers early.
    /// </summary>
    public static bool WarmUpVendorSdkOnDevicesAppear => true;

    /// <summary>
    /// The Veepoo manager used by the E580/E585 crashes on some phones when a successful
    /// <c>disconnectWatch</c> is followed by a second <c>connectDevice</c> in the same app
    /// process. In exclusive mode, Devices Disconnect is therefore a logical disconnect:
    /// clear RaphCare's Connected state but retain the vendor session for a safe reconnect.
    /// The session is naturally released when Android terminates the process.
    /// </summary>
    public static bool KeepVendorSessionAliveAfterUserDisconnect => PreferExclusiveVendorSession;

    /// <summary>
    /// After an auto-reconnect timeout/failure, wait before trying again on Devices appear.
    /// Repeated Veepoo connectDevice calls without a cool-down leave the radio hung.
    /// </summary>
    public static readonly TimeSpan ClaimedWatchReconnectFailureCooldown = TimeSpan.FromMinutes(2);

    public static bool ShouldSkipClaimedWatchReconnectAfterRecentFailure(
        DateTimeOffset? lastFailureUtc,
        DateTimeOffset utcNow) =>
        lastFailureUtc is not null
        && utcNow - lastFailureUtc.Value < ClaimedWatchReconnectFailureCooldown;

    /// <summary>
    /// Legacy name kept for older call sites. Prefer <see cref="PreferExclusiveVendorSession"/>.
    /// </summary>
    public static bool TryVendorSdkOnConnect => PreferExclusiveVendorSession;

    /// <summary>How long Measure waits for the first heart rate or oxygen sample after handshake.</summary>
    public static TimeSpan LiveMeasureTimeout { get; } = TimeSpan.FromSeconds(40);

    /// <summary>
    /// Vendor connect + notify must finish within this window or Measure aborts.
    /// Includes room for a few REQUEST_CANCELED (-2) retries after GATT handoff.
    /// </summary>
    public static TimeSpan VendorHandshakeTimeout { get; } = TimeSpan.FromSeconds(45);

    /// <summary>Pause after dropping Plugin.BLE so the vendor stack can reclaim the radio.</summary>
    public static TimeSpan PostGattDisconnectSettle { get; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// After exclusive Veepoo <c>disconnectWatch</c>, wait the same window before
    /// <c>connectDevice</c>. Immediate Disconnect then Connect was force-closing Android.
    /// </summary>
    public static TimeSpan PostVendorDisconnectSettle { get; } = PostGattDisconnectSettle;

    /// <summary>
    /// Remaining delay before a safe vendor reconnect after <paramref name="lastVendorDisconnectUtc"/>.
    /// Zero when no recent disconnect or the settle window has already elapsed.
    /// </summary>
    public static TimeSpan RemainingVendorReconnectSettle(
        DateTimeOffset? lastVendorDisconnectUtc,
        DateTimeOffset utcNow)
    {
        if (lastVendorDisconnectUtc is null)
            return TimeSpan.Zero;

        var elapsed = utcNow - lastVendorDisconnectUtc.Value;
        if (elapsed >= PostVendorDisconnectSettle)
            return TimeSpan.Zero;

        return PostVendorDisconnectSettle - elapsed;
    }

    /// <summary>How many times Measure retries vendor connect after Inuker REQUEST_CANCELED (-2).</summary>
    public static int VendorConnectMaxAttempts { get; } = 2;

    /// <summary>
    /// Calling Veepoo <c>disconnectWatch</c> / <c>stopDetect*</c> before any successful vendor
    /// connect triggers Android "This feature is not supported" and can kill the process.
    /// </summary>
    public static bool MustNotCallVendorDisconnectWithoutSession => true;

    /// <summary>Whether a vendor teardown JNI call is allowed for this session state.</summary>
    public static bool ShouldInvokeVendorDisconnect(bool sessionReady, bool connectStarted) =>
        sessionReady || connectStarted;

    /// <summary>
    /// Veepoo <c>stopDetectHeart</c> / <c>stopDetectSPO2H</c> before any matching
    /// <c>startDetect*</c> can toast and kill the Android process.
    /// </summary>
    public static bool MustNotCallVendorStopDetectWithoutStart => true;

    /// <summary>Whether a vendor stopDetect JNI call is allowed.</summary>
    public static bool ShouldInvokeVendorStopDetect(bool detectStarted) => detectStarted;

    /// <summary>
    /// When true, Measure uses Veepoo <c>startDetectHeart</c> on an exclusive vendor session.
    /// Requires exclusive Connect or a successful vendor-scan probe session first
    /// (no mid-Measure radio steal when PreferExclusive is off).
    /// </summary>
    public static bool EnableVendorLiveMeasure =>
        PreferExclusiveVendorSession || UseVeepooNativeScanProbe;

    /// <summary>
    /// Coordinator flag can lag the bridge after auto-reconnect. Adopt the bridge session for Measure.
    /// </summary>
    public static bool ShouldAdoptBridgeVendorSession(
        bool coordinatorUsingVendorSession,
        bool bridgeSessionReady) =>
        !coordinatorUsingVendorSession && bridgeSessionReady;

    /// <summary>
    /// Mid-Measure Plugin.BLE → Veepoo handshake force-closed the app when GATT was still up.
    /// Handshake during Measure is allowed only when there is no Plugin.BLE GATT to steal.
    /// </summary>
    public static bool ShouldEstablishVendorSessionForMeasure(
        bool enableVendorLiveMeasure,
        bool preferExclusiveVendorSession,
        bool vendorSdkAvailable,
        bool alreadyUsingVendorSession,
        bool hasBluetoothMac,
        bool pluginBleGattConnected) =>
        enableVendorLiveMeasure
        && preferExclusiveVendorSession
        && vendorSdkAvailable
        && !alreadyUsingVendorSession
        && hasBluetoothMac
        && AllowVendorConnectDuringMeasureWhenNoGatt
        && !pluginBleGattConnected;

    /// <summary>
    /// When true, Measure starts SpO₂ after the first heart-rate sample.
    /// Keep off: Veepoo often crashes if SpO₂ starts while heart detect is still running.
    /// </summary>
    public static bool EnableVendorSpo2DuringMeasure => false;

    /// <summary>
    /// When true, Measure reconnects Plugin.BLE after vendor detect.
    /// Keep off with exclusive vendor sessions: restoring GATT mid-lifecycle reintroduced crashes.
    /// </summary>
    public static bool RestorePluginBleAfterMeasure => false;

    /// <summary>Patient-facing copy when vendor Measure is gated off.</summary>
    public static string VendorLiveMeasureDisabledPatientMessage { get; } =
        "Live Measure through the watch SDK is temporarily off. It was closing the app on some phones. Keep your watch Connected on Devices. We will turn Measure back on in a later build.";

    /// <summary>Total Measure budget (handshake settle + sample wait).</summary>
    public static TimeSpan LiveMeasureOverallTimeout { get; } =
        VendorHandshakeTimeout + LiveMeasureTimeout + PostGattDisconnectSettle + TimeSpan.FromSeconds(20);
}
