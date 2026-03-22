using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace RaphCare.Mobile.Core.Shared.Navigation;

/// <summary>
/// Shell navigation must run on the main thread. Gesture commands and continuations after
/// <c>ConfigureAwait(false)</c> may run on worker threads — <c>Shell.Current.GoToAsync</c> then crashes on Android.
/// </summary>
public static class SafeShellNavigator
{
    public static Task GoToAsync(string route)
    {
        if (MainThread.IsMainThread)
            return Shell.Current.GoToAsync(route);

        return MainThread.InvokeOnMainThreadAsync(async () => await Shell.Current.GoToAsync(route).ConfigureAwait(true));
    }
}
