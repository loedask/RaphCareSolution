using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace RaphCare.Mobile.Core.Common.Navigation;

/// <summary>
/// Shell navigation must run on the main thread. Gesture commands and continuations after
/// <c>ConfigureAwait(false)</c> may run on worker threads — <c>Shell.Current.GoToAsync</c> then crashes on Android.
/// Unhandled navigation failures (null Shell, bad route, absolute push-only route) also close the process on Android.
/// </summary>
public static class SafeShellNavigator
{
    /// <summary>
    /// Marshals to the main thread, skips when Shell is missing, and swallows navigation exceptions
    /// so a failed tap does not kill the app.
    /// </summary>
    public static Task GoToAsync(string route) =>
        MainThread.InvokeOnMainThreadAsync(async () =>
        {
            try
            {
                var shell = Shell.Current;
                if (shell is null)
                {
                    Debug.WriteLine($"SafeShellNavigator: Shell.Current is null for route '{route}'.");
                    return;
                }

                if (LooksAbsolute(route) && !AbsoluteShellRouteRules.IsSafeAbsoluteTarget(route))
                {
                    var relative = ToRelativeRoute(route);
                    Debug.WriteLine(
                        $"SafeShellNavigator: rewriting unsafe absolute route '{route}' to '{relative}'.");
                    await shell.GoToAsync(relative).ConfigureAwait(true);
                    return;
                }

                await shell.GoToAsync(route).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SafeShellNavigator: navigation failed for '{route}': {ex}");
            }
        });

    private static bool LooksAbsolute(string route) =>
        route.StartsWith("//", StringComparison.Ordinal);

    private static string ToRelativeRoute(string route)
    {
        var trimmed = route.Trim();
        if (trimmed.StartsWith("//", StringComparison.Ordinal))
            trimmed = trimmed[2..];
        return trimmed;
    }
}
