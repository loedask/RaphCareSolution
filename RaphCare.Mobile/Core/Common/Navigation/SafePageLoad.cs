using Microsoft.Maui.Controls;

namespace RaphCare.Mobile.Core.Common.Navigation;

/// <summary>
/// Shared guard for <c>async void OnAppearing</c> loads. An unhandled exception from
/// <c>async void</c> closes the Android process.
/// </summary>
public static class SafePageLoad
{
    public static async Task RunAsync(Func<Task> load)
    {
        ArgumentNullException.ThrowIfNull(load);
        try
        {
            await load().ConfigureAwait(true);
        }
        catch
        {
            // Swallow: page stays open; ViewModels should surface ErrorMessage when possible.
        }
    }
}
