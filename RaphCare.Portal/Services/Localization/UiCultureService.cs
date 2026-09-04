using Microsoft.JSInterop;

namespace RaphCare.Portal.Services.Localization;

/// <inheritdoc />
public sealed class UiCultureService : IUiCultureService
{
    private readonly IJSRuntime _js;
    private bool _initialized;

    public UiCultureService(IJSRuntime js) => _js = js;

    public string CurrentCode { get; private set; } = "en";

    public event Action? CultureChanged;

    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        string? stored = null;
        try
        {
            stored = await _js.InvokeAsync<string?>("raphCareLocale.get").ConfigureAwait(true);
        }
        catch (JSException)
        {
            // JS not ready yet; keep default English.
        }

        CurrentCode = WebLanguagePreference.NormalizeCode(stored);
        WebLanguagePreference.ApplyCulture(CurrentCode);
        await SyncDocumentLangAsync().ConfigureAwait(true);
        _initialized = true;
        CultureChanged?.Invoke();
    }

    public async Task SetCultureAsync(string code)
    {
        CurrentCode = WebLanguagePreference.NormalizeCode(code);
        WebLanguagePreference.ApplyCulture(CurrentCode);

        try
        {
            await _js.InvokeVoidAsync("raphCareLocale.set", CurrentCode).ConfigureAwait(true);
        }
        catch (JSException)
        {
            // Preference still applied for this session.
        }

        await SyncDocumentLangAsync().ConfigureAwait(true);
        CultureChanged?.Invoke();
    }

    private async Task SyncDocumentLangAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("raphCareLocale.setDocumentLang", CurrentCode).ConfigureAwait(true);
        }
        catch (JSException)
        {
        }
    }
}
