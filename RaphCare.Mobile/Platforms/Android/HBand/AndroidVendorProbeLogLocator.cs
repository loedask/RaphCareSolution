#if ANDROID
namespace RaphCare.Mobile.Platforms.Android.HBand;

/// <summary>
/// Places the vendor probe trail under the app's external files dir so it can be
/// copied via Files / USB file transfer without adb (Android/data/.../files/).
/// </summary>
public static class AndroidVendorProbeLogLocator
{
    public const string TrailFileName = "raphcare-vendor-probe-log.txt";

    /// <summary>
    /// e.g. /storage/emulated/0/Android/data/com.yindula.raphcare/files/raphcare-vendor-probe-log.txt
    /// </summary>
    public static string? GetExternalFilesTrailPath()
    {
        var ctx = global::Android.App.Application.Context;
        var dir = ctx.GetExternalFilesDir(null)
                  ?? ctx.GetExternalFilesDir(global::Android.OS.Environment.DirectoryDocuments);
        if (dir is null)
            return null;

        return Path.Combine(dir.AbsolutePath, TrailFileName);
    }
}
#endif
