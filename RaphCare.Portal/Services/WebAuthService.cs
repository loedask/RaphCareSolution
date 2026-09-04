using System.Text;
using System.Text.Json;
using Microsoft.JSInterop;

namespace RaphCare.Portal.Services;

public interface IWebAuthService
{
    Task EnsureHydratedAsync();
    Task<WebAccountKind> GetAccountKindAsync();
    Task<string?> GetTokenAsync();
    Task<bool> IsSessionValidAsync();
    Task StoreSessionAsync(string token, WebAccountKind kind);
    Task SignOutAsync();
}

public sealed class WebAuthService(IJSRuntime js) : IWebAuthService
{
    private bool _hydrated;
    private string? _cachedToken;
    private WebAccountKind _cachedKind = WebAccountKind.None;

    public async Task EnsureHydratedAsync()
    {
        if (_hydrated)
            return;

        var kind = await js.InvokeAsync<string?>("raphCareAuth.getAccountKind").ConfigureAwait(true);
        _cachedKind = kind switch
        {
            "patient" => WebAccountKind.Patient,
            "professional" => WebAccountKind.Professional,
            _ => WebAccountKind.None
        };
        _cachedToken = await js.InvokeAsync<string?>("raphCareAuth.getToken").ConfigureAwait(true);
        _hydrated = true;
    }

    public async Task<WebAccountKind> GetAccountKindAsync()
    {
        await EnsureHydratedAsync().ConfigureAwait(true);
        return _cachedKind;
    }

    public async Task<string?> GetTokenAsync()
    {
        await EnsureHydratedAsync().ConfigureAwait(true);
        return _cachedToken;
    }

    public async Task<bool> IsSessionValidAsync()
    {
        await EnsureHydratedAsync().ConfigureAwait(true);
        if (string.IsNullOrWhiteSpace(_cachedToken) || _cachedKind == WebAccountKind.None)
            return false;

        return IsTokenNotExpired(_cachedToken);
    }

    public async Task StoreSessionAsync(string token, WebAccountKind kind)
    {
        _cachedToken = token;
        _cachedKind = kind;
        _hydrated = true;

        var kindValue = kind == WebAccountKind.Patient ? "patient" : "professional";
        await js.InvokeVoidAsync("raphCareAuth.setToken", token, kindValue).ConfigureAwait(true);
    }

    public async Task SignOutAsync()
    {
        _cachedToken = null;
        _cachedKind = WebAccountKind.None;
        _hydrated = true;
        await js.InvokeVoidAsync("raphCareAuth.clear").ConfigureAwait(true);
    }

    internal static bool IsTokenNotExpired(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2)
                return false;

            var base64 = parts[1].Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }

            using var doc = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(base64)));
            if (!doc.RootElement.TryGetProperty("exp", out var expElement))
                return false;

            var expSeconds = expElement.GetInt64();
            const long skewSeconds = 120;
            return expSeconds > DateTimeOffset.UtcNow.ToUnixTimeSeconds() - skewSeconds;
        }
        catch
        {
            return false;
        }
    }
}
