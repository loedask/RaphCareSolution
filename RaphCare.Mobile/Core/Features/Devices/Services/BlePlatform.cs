namespace RaphCare.Mobile.Core.Features.Devices.Services;

/// <summary>BLE is implemented for Android, iOS, and Mac Catalyst. Windows uses a placeholder until a validated UX exists.</summary>
internal static class BlePlatform
{
    public static bool IsSupported =>
#if WINDOWS
        false;
#else
        true;
#endif
}
