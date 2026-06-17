namespace RaphCare.Mobile.Core.Features.Devices.HBand;

/// <summary>
/// Pointers to the public HBandSDK GitHub ecosystem used by many E580/E585-class bands.
/// RaphCare’s default BLE implementation uses Plugin.BLE; full vendor protocol support requires the native SDKs — see docs/12_HBand_SDK_Integration.md.
/// </summary>
public static class HBandSdkInfo
{
    public const string OrganizationUrl = "https://github.com/HBandSDK";
    public const string AndroidSdkRepoUrl = "https://github.com/HBandSDK/Android_Ble_SDK";
    public const string IOSdkRepoUrl = "https://github.com/HBandSDK/iOS_Ble_SDK";

    /// <summary>Password often used in vendor SDK samples after <c>connectDevice</c>; confirm for your fleet.</summary>
    public const string DefaultDevicePasswordPlaceholder = "0000";
}
