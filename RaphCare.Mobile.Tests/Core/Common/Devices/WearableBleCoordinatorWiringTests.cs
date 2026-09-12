using Xunit;

namespace RaphCare.Mobile.Tests.Core.Common.Devices;

/// <summary>
/// Mobile.Tests cannot reference RaphCare.Mobile (MAUI). These checks lock the coordinator
/// call sites so the Scan / Measure fixes cannot be removed without a failing test.
/// </summary>
public sealed class WearableBleCoordinatorWiringTests
{
    [Fact]
    public void ManualScanMustCallReleaseAndClaimedMacFilter()
    {
        var text = ReadCoordinatorSource();

        Assert.Contains("ShouldReleaseActiveLinksBeforeManualScan", text, StringComparison.Ordinal);
        Assert.Contains("ReleaseActiveLinksForManualScanAsync", text, StringComparison.Ordinal);
        Assert.Contains("IncludeInFilteredScan", text, StringComparison.Ordinal);
        Assert.Contains("UsePermissivePluginBleScanFilter", text, StringComparison.Ordinal);
        Assert.Contains("SeedPairedOrConnectedDevices", text, StringComparison.Ordinal);
        Assert.Contains("GetSystemConnectedOrPairedDevices", text, StringComparison.Ordinal);
        Assert.Contains("ShouldShowClaimedWatchFallback", text, StringComparison.Ordinal);
        Assert.Contains("VendorSessionPlaceholderId", text, StringComparison.Ordinal);
        Assert.Contains("RemainingVendorReconnectSettle", text, StringComparison.Ordinal);
        Assert.Contains("WaitForVendorReconnectSettleAsync", text, StringComparison.Ordinal);
        Assert.Contains("ResolveClaimedDeviceName", text, StringComparison.Ordinal);
        Assert.Contains("TryConnectClaimedWatchViaPluginBleAsync", text, StringComparison.Ordinal);
        Assert.Contains("KeepVendorSessionAliveAfterUserDisconnect", text, StringComparison.Ordinal);
        Assert.Contains("retainedVendorSession", text, StringComparison.Ordinal);
        Assert.Contains("preferredMac", text, StringComparison.Ordinal);
    }

    [Fact]
    public void ClaimedWearableConnectMustReuseRetainedVendorSession()
    {
        // After logical Disconnect, Connect on the Claimed wearable row must adopt the
        // live Veepoo session instead of calling connectDevice again.
        var text = ReadCoordinatorSource();
        var placeholderIdx = text.IndexOf(
            "deviceId == VendorSessionPlaceholderId",
            StringComparison.Ordinal);
        Assert.True(placeholderIdx >= 0, "Claimed wearable Connect path not found.");
        var nextPathIdx = text.IndexOf(
            "if (!_devices.TryGetValue(deviceId",
            placeholderIdx,
            StringComparison.Ordinal);
        Assert.True(nextPathIdx > placeholderIdx, "Could not bound placeholder Connect body.");
        var body = text[placeholderIdx..nextPathIdx];
        Assert.Contains("ShouldReuseExistingVendorSession", body, StringComparison.Ordinal);
        var reuseIdx = body.IndexOf("ShouldReuseExistingVendorSession", StringComparison.Ordinal);
        var handshakeIdx = body.IndexOf("ConnectAndHandshakeAsync", StringComparison.Ordinal);
        Assert.True(reuseIdx >= 0 && handshakeIdx > reuseIdx,
            "Reuse check must run before ConnectAndHandshake on the Claimed wearable path.");
    }

    [Fact]
    public void ManualScanMustNotTearDownVendorSession()
    {
        // Regression: ReleaseActiveLinksForManualScanAsync called _hband.DisconnectAsync,
        // so Scan → disconnectWatch → Connect force-closed the app.
        var text = ReadCoordinatorSource();
        var methodIdx = text.IndexOf(
            "private async Task ReleaseActiveLinksForManualScanAsync",
            StringComparison.Ordinal);
        Assert.True(methodIdx >= 0, "ReleaseActiveLinksForManualScanAsync not found.");
        var nextMethodIdx = text.IndexOf(
            "\n    private ",
            methodIdx + 1,
            StringComparison.Ordinal);
        Assert.True(nextMethodIdx > methodIdx, "Could not bound ReleaseActiveLinksForManualScanAsync.");
        var body = text[methodIdx..nextMethodIdx];
        Assert.DoesNotContain("await _hband.DisconnectAsync", body, StringComparison.Ordinal);
        Assert.Contains("ReleasePluginBleLinkAsync", body, StringComparison.Ordinal);
        Assert.Contains("Never call _hband.DisconnectAsync here", body, StringComparison.Ordinal);
    }

    [Fact]
    public void DevicesPageScanMustPassClaimedBluetoothMac()
    {
        var text = ReadDevicesViewModelSource();

        Assert.Contains("StartScanAsync(ShowAllDevices, ClaimedBluetoothMac", text, StringComparison.Ordinal);
        Assert.Contains("IsLiveMeasureSessionReady", text, StringComparison.Ordinal);
        Assert.Contains("IsMissingClaimedBluetoothMacForFallback", text, StringComparison.Ordinal);
        Assert.DoesNotContain("ShouldAutoConnectClaimedWatchAfterManualScan", text, StringComparison.Ordinal);
        Assert.Contains("CheckBluetoothPermissionsAsync", text, StringComparison.Ordinal);
        Assert.Contains("ShouldReconnectClaimedWatchOnAppearWithPermission", text, StringComparison.Ordinal);
        Assert.Contains("LoadClaimedDevicesAsync", text, StringComparison.Ordinal);
    }

    [Fact]
    public void CrashProbeMustBeConsumedOnDevicesAndWatchReadingsBeforeAutoReconnect()
    {
        // Regression: 1.8.45 paused reconnect on Devices only. Watch readings still called
        // ReconnectClaimedWatchAsync unconditionally and wiped or re-crashed the breadcrumb.
        var devices = ReadDevicesViewModelSource();
        var watch = ReadWatchReadingsViewModelSource();
        var coordinator = ReadCoordinatorSource();

        Assert.Contains("TryConsumeVendorConnectCrashMessage", coordinator, StringComparison.Ordinal);
        Assert.Contains("TryConsumeVendorConnectCrashMessage", devices, StringComparison.Ordinal);
        Assert.Contains("TryConsumeVendorConnectCrashMessage", watch, StringComparison.Ordinal);
        Assert.Contains("ShouldSkipClaimedWatchReconnectAfterCrashProbe", devices, StringComparison.Ordinal);
        Assert.Contains("ShouldSkipClaimedWatchReconnectAfterCrashProbe", watch, StringComparison.Ordinal);

        var watchAppearIdx = watch.IndexOf("public async Task OnAppearingAsync", StringComparison.Ordinal);
        Assert.True(watchAppearIdx >= 0);
        var reconnectIdx = watch.IndexOf("ReconnectClaimedWatchAsync", watchAppearIdx, StringComparison.Ordinal);
        var consumeIdx = watch.IndexOf("TryConsumeVendorConnectCrashMessage", watchAppearIdx, StringComparison.Ordinal);
        Assert.True(consumeIdx > watchAppearIdx && reconnectIdx > consumeIdx,
            "Watch readings must consume the crash probe before ReconnectClaimedWatchAsync.");
    }

    [Fact]
    public void VendorNativeScanProbeMustNotStartPluginBleScan()
    {
        var text = ReadCoordinatorSource();
        var methodIdx = text.IndexOf(
            "public async Task ConnectViaVendorScanProbeAsync",
            StringComparison.Ordinal);
        Assert.True(methodIdx >= 0, "ConnectViaVendorScanProbeAsync not found.");
        var nextMethodIdx = text.IndexOf(
            "\n    public async Task ConnectAsync",
            methodIdx + 1,
            StringComparison.Ordinal);
        Assert.True(nextMethodIdx > methodIdx, "Could not bound ConnectViaVendorScanProbeAsync.");
        var body = text[methodIdx..nextMethodIdx];
        Assert.DoesNotContain("StartScanningForDevicesAsync", body, StringComparison.Ordinal);
        Assert.DoesNotContain("ConnectToDeviceAsync", body, StringComparison.Ordinal);
        Assert.Contains("StartVendorScanAsync", body, StringComparison.Ordinal);
        Assert.Contains("ConnectAndHandshakeAsync", body, StringComparison.Ordinal);
        Assert.Contains("StepScanKeep", body, StringComparison.Ordinal);
        Assert.DoesNotContain("SettleAfterScanStopBeforeVendorConnectAsync", body, StringComparison.Ordinal);
        // Must not stop vendor scan before Connect on this probe (1.8.52 keep-warm).
        var matchIdx = body.IndexOf("var (mac, name)", StringComparison.Ordinal);
        Assert.True(matchIdx >= 0);
        var connectIdx = body.IndexOf("ConnectAndHandshakeAsync", matchIdx, StringComparison.Ordinal);
        Assert.True(connectIdx > matchIdx);
        var betweenMatchAndConnect = body[matchIdx..connectIdx];
        Assert.DoesNotContain("StopVendorScanAsync", betweenMatchAndConnect, StringComparison.Ordinal);
        Assert.Contains("UseVeepooNativeScanProbe", body, StringComparison.Ordinal);
        Assert.Contains("ReleaseActivePluginBleWithoutVendorHandoffAsync", body, StringComparison.Ordinal);

        var devices = ReadDevicesViewModelSource();
        Assert.Contains("ConnectViaVendorScanProbeAsync", devices, StringComparison.Ordinal);
        Assert.Contains("VendorScanProbeCommand", devices, StringComparison.Ordinal);
        Assert.Contains("ShowVendorScanProbe", devices, StringComparison.Ordinal);
        Assert.Contains("ShareProbeLogCommand", devices, StringComparison.Ordinal);
        Assert.Contains("ProbeSelfTestCommand", devices, StringComparison.Ordinal);
        Assert.Contains("ShareVendorProbeTrailAsync", text, StringComparison.Ordinal);
        Assert.Contains("raphcare-vendor-probe-log.txt", File.ReadAllText(FindRepoFile(
            Path.Combine("RaphCare.Mobile", "Core", "Features", "Devices", "HBand", "FileVendorConnectStepProbe.cs"))), StringComparison.Ordinal);
    }

    [Fact]
    public void VendorScanCreateProxyMustRequireJavaInterface()
    {
        // Defense: Proxy.NewProxyInstance only accepts interfaces. A loaded abstract class
        // must not lock CreateProxy before fallbacks (see docs/12 after 1.8.47 crash review).
        var text = File.ReadAllText(FindRepoFile(
            Path.Combine(
                "RaphCare.Mobile",
                "Platforms",
                "Android",
                "HBand",
                "HBandAndroidWearableBridge.cs")));
        Assert.Contains("IsInterface", text, StringComparison.Ordinal);
        Assert.Contains("SCAN-PROXY-FAILED", text, StringComparison.Ordinal);
        Assert.Contains("ShouldUseTimedStartScanOverload", text, StringComparison.Ordinal);
        Assert.Contains("ConnectStepChanged", text, StringComparison.Ordinal);
    }

    [Fact]
    public void VendorScanProbeCommandMustRefreshCanExecuteAfterClaimLoads()
    {
        // Regression: 1.8.46 left VendorScanProbeCommand CanExecute stuck false after
        // RegisteredDeviceId was set on Devices appear (Scan was refreshed; diagnostic was not).
        var text = ReadDevicesViewModelSource();
        Assert.Contains("CanVendorScanProbe", text, StringComparison.Ordinal);
        Assert.Contains("RaiseBleCommandStates", text, StringComparison.Ordinal);

        var claimSetterIdx = text.IndexOf("public Guid? RegisteredDeviceId", StringComparison.Ordinal);
        Assert.True(claimSetterIdx >= 0);
        var nextPropIdx = text.IndexOf("public string? ClaimedBluetoothMac", claimSetterIdx, StringComparison.Ordinal);
        Assert.True(nextPropIdx > claimSetterIdx);
        var claimBody = text[claimSetterIdx..nextPropIdx];
        Assert.Contains("RaiseBleCommandStates", claimBody, StringComparison.Ordinal);
        Assert.Contains("VendorScanProbeCommand", text[text.IndexOf("private void RaiseBleCommandStates", StringComparison.Ordinal)..], StringComparison.Ordinal);
    }

    [Fact]
    public void VendorConnectMustLogConnectDeviceMarkersForCrashDiagnosis()
    {
        // Partner: Connect force-closed after Scan. Markers prove whether connectDevice returns.
        var text = File.ReadAllText(FindRepoFile(
            Path.Combine(
                "RaphCare.Mobile",
                "Platforms",
                "Android",
                "HBand",
                "HBandAndroidWearableBridge.cs")));
        Assert.Contains("CONNECT-1 native check:", text, StringComparison.Ordinal);
        Assert.Contains("CONNECT-2 calling connectDevice:", text, StringComparison.Ordinal);
        Assert.Contains("CONNECT-3 connectDevice returned:", text, StringComparison.Ordinal);
        Assert.Contains("TryInvokeConnectDevice", text, StringComparison.Ordinal);
        Assert.Contains("PreferOfficialMacOnlyConnectDeviceOverload", text, StringComparison.Ordinal);
        Assert.Contains("CONNECT-INVOKE", text, StringComparison.Ordinal);
        Assert.Contains("INIT-1", text, StringComparison.Ordinal);
        Assert.Contains("INIT-3", text, StringComparison.Ordinal);
        Assert.Contains("SCAN-1", text, StringComparison.Ordinal);
        Assert.Contains("SCAN-INVOKE", text, StringComparison.Ordinal);
        Assert.Contains("SCAN-RESULT", text, StringComparison.Ordinal);
        Assert.Contains("startScanDevice", text, StringComparison.Ordinal);
        Assert.Contains("getMangerInstance", text, StringComparison.Ordinal);
        Assert.Contains("HasBluetoothClient", text, StringComparison.Ordinal);
        Assert.Contains("WarmUpAsync", text, StringComparison.Ordinal);
        Assert.Contains("MarkConnectStep", text, StringComparison.Ordinal);
        Assert.Contains("VendorConnectCrashProbeRules", text, StringComparison.Ordinal);
        Assert.Contains("WarmUpVendorSdkAsync", File.ReadAllText(FindRepoFile(
            Path.Combine("RaphCare.Mobile", "Core", "Features", "Devices", "Services", "WearableBleCoordinator.cs"))), StringComparison.Ordinal);
        Assert.Contains("SettleAfterScanStopBeforeVendorConnectAsync", File.ReadAllText(FindRepoFile(
            Path.Combine("RaphCare.Mobile", "Core", "Features", "Devices", "Services", "WearableBleCoordinator.cs"))), StringComparison.Ordinal);
        var csproj = File.ReadAllText(FindRepoFile(Path.Combine("RaphCare.Mobile", "RaphCare.Mobile.csproj")));
        Assert.Contains("mcumgr-core-2.7.4.aar", csproj, StringComparison.Ordinal);
        Assert.Contains("mcumgr-ble-2.7.4.aar", csproj, StringComparison.Ordinal);
        Assert.Contains("ble-2.11.0.aar", csproj, StringComparison.Ordinal);
        Assert.Contains("slf4j-api-2.0.17.jar", csproj, StringComparison.Ordinal);
        Assert.Contains("download-hband-nordic-mcumgr-libs.ps1", File.ReadAllText(FindRepoFile(
            Path.Combine("tools", "download-hband-android-libs.ps1"))), StringComparison.Ordinal);
    }

    [Fact]
    public void MeasureMustNotStopDetectBeforeFirstStartDetect()
    {
        var text = ReadCoordinatorSource();
        var measureIdx = text.IndexOf(
            "public async Task<WearableVitalsSnapshot?> MeasureLiveVitalsAsync",
            StringComparison.Ordinal);
        Assert.True(measureIdx >= 0, "MeasureLiveVitalsAsync not found.");

        var startIdx = text.IndexOf(
            "StartLiveHeartRateAsync",
            measureIdx,
            StringComparison.Ordinal);
        Assert.True(startIdx > measureIdx, "Measure must call StartLiveHeartRateAsync.");

        // Inside Measure, the first heart start must not be preceded by a stop-detect call
        // on a fresh session (that sequence force-closed the app).
        var measureBody = text[measureIdx..startIdx];
        Assert.DoesNotContain("StopLiveDetectionsAsync", measureBody, StringComparison.Ordinal);
        Assert.Contains(
            "Never call stopDetect",
            measureBody,
            StringComparison.Ordinal);
    }

    private static string ReadCoordinatorSource() =>
        File.ReadAllText(FindRepoFile(
            Path.Combine(
                "RaphCare.Mobile",
                "Core",
                "Features",
                "Devices",
                "Services",
                "WearableBleCoordinator.cs")));

    private static string ReadDevicesViewModelSource() =>
        File.ReadAllText(FindRepoFile(
            Path.Combine(
                "RaphCare.Mobile",
                "Core",
                "Features",
                "Devices",
                "ViewModels",
                "DevicesViewModel.cs")));

    private static string ReadWatchReadingsViewModelSource() =>
        File.ReadAllText(FindRepoFile(
            Path.Combine(
                "RaphCare.Mobile",
                "Core",
                "Features",
                "Devices",
                "ViewModels",
                "WatchReadingsViewModel.cs")));

    private static string FindRepoFile(string relativePath)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, relativePath);
            if (File.Exists(candidate))
                return candidate;

            var sln = Path.Combine(dir.FullName, "RaphCareSolution.slnx");
            if (File.Exists(sln))
            {
                candidate = Path.Combine(dir.FullName, relativePath);
                if (File.Exists(candidate))
                    return candidate;
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException($"Could not locate {relativePath} from test output directory.");
    }
}
