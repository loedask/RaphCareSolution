using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace RaphCare.Mobile.Core.Features.Settings.Models;

/// <summary>Selectable preset chip on Medical information.</summary>
public sealed class MedicalInfoChipItem : INotifyPropertyChanged
{
    private bool _isSelected;
    private readonly Action<MedicalInfoChipItem> _toggled;

    public MedicalInfoChipItem(string id, string label, bool isExclusive, Action<MedicalInfoChipItem> toggled)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Label = label ?? throw new ArgumentNullException(nameof(label));
        IsExclusive = isExclusive;
        _toggled = toggled ?? throw new ArgumentNullException(nameof(toggled));
        ToggleCommand = new Command(() => _toggled(this));
    }

    public string Id { get; }
    public string Label { get; }
    public bool IsExclusive { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
                return;
            _isSelected = value;
            OnPropertyChanged();
        }
    }

    public ICommand ToggleCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
