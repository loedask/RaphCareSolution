using Microsoft.Maui.ApplicationModel;

namespace RaphCare.Mobile.Core.Features.Devices.Services;

/// <summary>Runtime BLE permissions on Android; other platforms rely on plist usage strings.</summary>
public sealed class BluetoothPermissions : Permissions.BasePlatformPermission
{
#if ANDROID
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        OperatingSystem.IsAndroidVersionAtLeast(31)
            ?
            [
                (global::Android.Manifest.Permission.BluetoothScan, true),
                (global::Android.Manifest.Permission.BluetoothConnect, true),
            ]
            :
            [
                (global::Android.Manifest.Permission.Bluetooth, true),
                (global::Android.Manifest.Permission.BluetoothAdmin, true),
                (global::Android.Manifest.Permission.AccessFineLocation, true),
            ];
#endif
}
