using RaphCare.Mobile.Kernel.Core.Common.Devices;

namespace RaphCare.Mobile.Core.Features.Devices.Services;

/// <summary>
/// Filters scan results to bracelets commonly sold as E580 / E585 (OEM names vary).
/// Prefer <see cref="WearableBleScanRules"/> for new call sites.
/// </summary>
public static class E585E580DeviceFilter
{
    /// <summary>Returns true if the advertised name suggests an E580/E585-class bracelet.</summary>
    public static bool Matches(string? deviceName) =>
        WearableBleScanRules.MatchesE580StyleName(deviceName);
}
