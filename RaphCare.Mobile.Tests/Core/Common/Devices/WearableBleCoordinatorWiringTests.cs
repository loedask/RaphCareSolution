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
        Assert.Contains("preferredMac", text, StringComparison.Ordinal);
    }

    [Fact]
    public void DevicesPageScanMustPassClaimedBluetoothMac()
    {
        var text = ReadDevicesViewModelSource();

        Assert.Contains("StartScanAsync(ShowAllDevices, ClaimedBluetoothMac", text, StringComparison.Ordinal);
        Assert.Contains("IsLiveMeasureSessionReady", text, StringComparison.Ordinal);
        Assert.Contains("IsMissingClaimedBluetoothMacForFallback", text, StringComparison.Ordinal);
        Assert.Contains("LoadClaimedDevicesAsync", text, StringComparison.Ordinal);
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
