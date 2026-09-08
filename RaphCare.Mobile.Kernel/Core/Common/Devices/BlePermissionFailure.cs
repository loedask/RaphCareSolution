namespace RaphCare.Mobile.Kernel.Core.Common.Devices;

/// <summary>
/// Detects Android BLE runtime permission failures from exception text.
/// </summary>
public static class BlePermissionFailure
{
    /// <summary>
    /// True when the message is the Android 12+ missing <c>BLUETOOTH_SCAN</c> / registerScanner failure.
    /// </summary>
    public static bool IsMissingNearbyDevicesPermission(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return false;

        return message.Contains("BLUETOOTH_SCAN", StringComparison.OrdinalIgnoreCase)
               || message.Contains("BLUETOOTH_CONNECT", StringComparison.OrdinalIgnoreCase)
               || message.Contains("registerScanner", StringComparison.OrdinalIgnoreCase)
               || message.Contains("Need android.permission.BLUETOOTH", StringComparison.OrdinalIgnoreCase);
    }
}
