using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Maui.ApplicationModel;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Common.ViewModels;

/// <summary>
/// Base class for MVVM ViewModels with property change notification and busy state.
/// </summary>
public abstract class BaseViewModel : INotifyPropertyChanged
{
    private bool _isBusy;
    private string? _title;
    private string? _busyMessage;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (!SetProperty(ref _isBusy, value))
                return;
            if (!value)
                BusyMessage = null;
        }
    }

    /// <summary>Optional message for <c>BusyOverlay</c> while <see cref="IsBusy"/> is true.</summary>
    public string? BusyMessage
    {
        get => _busyMessage;
        set => SetProperty(ref _busyMessage, value);
    }

    public string? Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    /// <returns><see langword="true"/> when the value changed.</returns>
    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;
        backingStore = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Raises <see cref="PropertyChanged"/> on the main thread so bindings stay valid after
    /// <c>await</c> with <c>ConfigureAwait(false)</c> (common in ViewModels).
    /// </summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        var handler = PropertyChanged;
        if (handler is null)
            return;

        if (MainThread.IsMainThread)
            handler(this, new PropertyChangedEventArgs(propertyName));
        else
            MainThread.BeginInvokeOnMainThread(() => handler(this, new PropertyChangedEventArgs(propertyName)));
    }

    /// <summary>Localized string for <see cref="CultureInfo.CurrentUICulture"/>.</summary>
    protected static string T(string name) =>
        AppResources.T(name, CultureInfo.CurrentUICulture);

    /// <summary>Formats a localized pattern with <see cref="CultureInfo.CurrentCulture"/>.</summary>
    protected static string Format(string format, params object?[] args) =>
        string.Format(CultureInfo.CurrentCulture, format, args);

    /// <summary>Parses a Shell query value as a <see cref="Guid"/> using invariant culture.</summary>
    protected static bool TryGetQueryGuid(IDictionary<string, object> query, string key, out Guid id)
    {
        id = default;
        if (!query.TryGetValue(key, out var value) || value is null)
            return false;

        return Guid.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out id);
    }

    /// <summary>
    /// Runs <paramref name="action"/> on the UI thread. Required before mutating
    /// <see cref="System.Collections.ObjectModel.ObservableCollection{T}"/> bound to MAUI views
    /// after <c>ConfigureAwait(false)</c>.
    /// </summary>
    protected static Task RunOnMainThreadAsync(Action action) =>
        MainThread.InvokeOnMainThreadAsync(action);

    /// <inheritdoc cref="RunOnMainThreadAsync(Action)"/>
    protected static Task RunOnMainThreadAsync(Func<Task> func) =>
        MainThread.InvokeOnMainThreadAsync(func);
}
