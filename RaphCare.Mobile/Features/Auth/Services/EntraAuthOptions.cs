namespace RaphCare.Mobile.Features.Auth.Services;

/// <summary>
/// Configuration for Microsoft Entra ID (B2C / External ID) used by the mobile app.
/// Should match the API's Entra options (Authority, ClientId, Audience).
/// </summary>
public class EntraAuthOptions
{
    public const string SectionName = "Entra";

    /// <summary>Application (client) ID of the app registration.</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>Tenant ID or "common" for multi-tenant.</summary>
    public string TenantId { get; set; } = "common";

    /// <summary>Full authority URL, e.g. https://login.microsoftonline.com/{tenant}/v2.0 or B2C policy URL.</summary>
    public string? Authority { get; set; }

    /// <summary>Redirect URI for interactive login (e.g. msal{ClientId}://auth for mobile).</summary>
    public string RedirectUri { get; set; } = "msal{ClientId}://auth";

    /// <summary>API scope to request (e.g. api://raphcare-api/.default).</summary>
    public string ApiScope { get; set; } = "api://raphcare-api/.default";

    public string GetAuthority() =>
        !string.IsNullOrWhiteSpace(Authority)
            ? Authority!.TrimEnd('/')
            : $"https://login.microsoftonline.com/{TenantId}/v2.0";

    public string GetRedirectUri() =>
        RedirectUri.Replace("{ClientId}", ClientId, StringComparison.OrdinalIgnoreCase);
}
