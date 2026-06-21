namespace RaphCare.Mobile.Core.Infrastructure.Composition;

/// <summary>
/// Resolves services for Shell <see cref="Microsoft.Maui.Controls.Shell"/> pages that must expose a parameterless constructor for XAML <c>DataTemplate</c>.
/// Prefers <see cref="Microsoft.Maui.Controls.Application.Current"/>.Handler.MauiContext.Services when available; falls back to the root provider set at startup (e.g. before the window is shown).
/// </summary>
public static class MobileServiceHub
{
    private static IServiceProvider? _root;

    internal static void SetRootProvider(IServiceProvider root) =>
        _root = root ?? throw new ArgumentNullException(nameof(root));

    public static T GetRequiredService<T>() where T : notnull
    {
        var contextual = Application.Current?.Handler?.MauiContext?.Services;
        if (contextual is not null)
            return contextual.GetRequiredService<T>();

        if (_root is not null)
            return _root.GetRequiredService<T>();

        throw new InvalidOperationException(
            "Services are not available yet. Ensure MauiApp has been built and MobileServiceHub.SetRootProvider was called.");
    }
}
