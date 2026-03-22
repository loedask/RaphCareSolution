using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RaphCare.Mobile.Core.Features.Auth.Models;

/// <summary>Single bar height for the in-recording visual (concept-style level meter).</summary>
public sealed class WaveBarItem : INotifyPropertyChanged
{
    private double _height = 8;

    public double Height
    {
        get => _height;
        set
        {
            if (Math.Abs(_height - value) < 0.01)
                return;
            _height = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
