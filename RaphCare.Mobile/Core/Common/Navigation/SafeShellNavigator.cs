using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace RaphCare.Mobile.Core.Common.Navigation;

/// <summary>
/// Shell navigation must run on the main thread. Gesture commands and continuations after
/// <c>ConfigureAwait(false)</c> may run on worker threads — <c>Shell.Current.GoToAsync</c> then crashes on Android.
/// </summary>
public static class SafeShellNavigator
{
    /// <summary>Always marshals to the main thread (gestures and continuations may not be on the UI thread on Android).</summary>
    public static Task GoToAsync(string route) =>
        MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(route));
}
