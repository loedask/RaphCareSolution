namespace RaphCare.Mobile.Kernel.Core.Common.Devices;

/// <summary>
/// After a watch is claimed with a Bluetooth MAC, Connect may only target that address.
/// Before the first successful pair (no MAC yet), any claimed serial may connect and then bind MAC.
/// </summary>
public static class ClaimedWatchConnectGate
{
    /// <summary>
    /// Returns false when the patient has a locked MAC and the nearby peripheral does not match it.
    /// </summary>
    public static bool AllowsConnect(string? claimedBluetoothMac, string? peripheralMac)
    {
        var expected = BluetoothMacNormalizer.TryNormalize(claimedBluetoothMac);
        if (expected is null)
            return true;

        var actual = BluetoothMacNormalizer.TryNormalize(peripheralMac);
        if (actual is null)
            return false;

        return BluetoothMacNormalizer.EqualsNormalized(expected, actual);
    }
}
