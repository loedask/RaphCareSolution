using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.ApplicationModel;

namespace RaphCare.Mobile.Core.Shared.ViewModels;

/// <summary>
/// Base class for MVVM ViewModels with property change notification and busy state.
/// </summary>
public abstract class BaseViewModel : INotifyPropertyChanged
{
    private bool _isBusy;
    private string? _title;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string? Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    protected void SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return;
        backingStore = value;
        OnPropertyChanged(propertyName);
    }

    /// <summary>
    /// Raises <see cref="PropertyChanged"/> on the main thread so bindings stay valid after
    /// <c>await</c> with <c>ConfigureAwait(false)</c> (common in ViewModels).
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        var handler = PropertyChanged;
        if (handler is null)
            return;

        if (MainThread.IsMainThread)
            handler(this, new PropertyChangedEventArgs(propertyName));
        else
            MainThread.BeginInvokeOnMainThread(() => handler(this, new PropertyChangedEventArgs(propertyName)));
    }
}
