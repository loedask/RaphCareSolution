namespace RaphCare.Portal.Services.Localization;

/// <summary>Persists and applies the Web UI language preference.</summary>
public interface IUiCultureService
{
    string CurrentCode { get; }

    event Action? CultureChanged;

    Task InitializeAsync();

    Task SetCultureAsync(string code);
}
