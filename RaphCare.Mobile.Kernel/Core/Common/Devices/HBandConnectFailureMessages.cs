namespace RaphCare.Mobile.Kernel.Core.Common.Devices;

/// <summary>
/// Patient-facing text for Veepoo/Inuker connect codes (see REQUEST_* in the vendor stack).
/// Code -2 is REQUEST_CANCELED when the radio is still busy after Plugin.BLE disconnect.
/// </summary>
public static class HBandConnectFailureMessages
{
    public const int RequestCanceled = -2;

    public static string ForCode(int code) =>
        code switch
        {
            RequestCanceled =>
                "HBand connect failed (code -2). The watch radio was still busy. Wait a few seconds, then Measure again.",
            -5 => "Bluetooth is off. Turn it on and try Measure again.",
            -6 => "Bluetooth service not ready. Wait a moment, then Measure again.",
            -7 => "HBand connect timed out. Keep the watch nearby and try again.",
            _ => $"HBand connect failed (code {code}).",
        };

    public static bool IsRadioBusyCancel(string? message) =>
        !string.IsNullOrWhiteSpace(message)
        && message.Contains("code -2", StringComparison.Ordinal);
}
