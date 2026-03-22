using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace RaphCare.Mobile.Core.Features.Auth;

/// <summary>
/// Shell navigation must run on the main thread. After <c>await</c> with <c>ConfigureAwait(false)</c>,
/// continuations may run on a worker thread — <c>Shell.Current.GoToAsync</c> then crashes on Android.
/// </summary>
internal static class AuthShellNavigator
{
    public static Task GoToAsync(string route)
    {
        if (MainThread.IsMainThread)
            return Shell.Current.GoToAsync(route);

        return MainThread.InvokeOnMainThreadAsync(async () => await Shell.Current.GoToAsync(route).ConfigureAwait(true));
    }
}
