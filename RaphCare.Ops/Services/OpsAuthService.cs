using Microsoft.JSInterop;
using RaphCare.Client.Contracts;

namespace RaphCare.Ops.Services;

public interface IOpsAuthService
{
    Task EnsureHydratedAsync();
    Task<string?> GetTokenAsync();
    Task<bool> IsSessionValidAsync();
    Task StoreSessionAsync(string token);
    Task SignOutAsync();
}

public sealed class OpsAuthService(IJSRuntime js) : IOpsAuthService
{
    private bool _hydrated;
    private string? _cachedToken;

    public async Task EnsureHydratedAsync()
    {
        if (_hydrated)
            return;

        _cachedToken = await js.InvokeAsync<string?>("raphCareOpsAuth.getToken").ConfigureAwait(true);
        _hydrated = true;
    }

    public async Task<string?> GetTokenAsync()
    {
        await EnsureHydratedAsync().ConfigureAwait(true);
        return _cachedToken;
    }

    public async Task<bool> IsSessionValidAsync()
    {
        await EnsureHydratedAsync().ConfigureAwait(true);
        if (string.IsNullOrWhiteSpace(_cachedToken))
            return false;

        return OpsTokenExpiry.IsTokenNotExpired(_cachedToken);
    }

    public async Task StoreSessionAsync(string token)
    {
        _cachedToken = token;
        _hydrated = true;
        await js.InvokeVoidAsync("raphCareOpsAuth.setToken", token).ConfigureAwait(true);
    }

    public async Task SignOutAsync()
    {
        _cachedToken = null;
        _hydrated = true;
        await js.InvokeVoidAsync("raphCareOpsAuth.clear").ConfigureAwait(true);
    }
}

public sealed class OpsAccessTokenProvider(IOpsAuthService auth) : IAccessTokenProvider
{
    public Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default) =>
        auth.GetTokenAsync();
}

internal static class OpsTokenExpiry
{
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

            using var doc = System.Text.Json.JsonDocument.Parse(
                System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64)));
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

internal static class VerificationCodeText
{
    public static string? NormalizeOrNull(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;
        var trimmed = code.Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }
}
