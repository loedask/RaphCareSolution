using Microsoft.Maui.Devices;

namespace RaphCare.Mobile.Core.Shared.Services.Auth;

/// <summary>
/// Configuration for Microsoft Entra ID (B2C / External ID) used by the mobile app.
/// </summary>
public class EntraAuthOptions
{
    public const string SectionName = "Entra";

    public string ClientId { get; set; } = string.Empty;
    public string TenantId { get; set; } = "common";
    public string? Authority { get; set; }
    public string RedirectUri { get; set; } = "msal{ClientId}://auth";
    public string ApiScope { get; set; } = "api://raphcare-api/.default";

    public string GetAuthority() =>
        !string.IsNullOrWhiteSpace(Authority)
            ? Authority!.TrimEnd('/')
            : $"https://login.microsoftonline.com/{TenantId}/v2.0";

    public string GetRedirectUri()
    {
        // MSAL on desktop/WinUI requires a loopback redirect URI (http://localhost)
        if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            return "http://localhost";
        }

        // For mobile platforms, use the MSAL custom scheme
        return RedirectUri.Replace("{ClientId}", ClientId, StringComparison.OrdinalIgnoreCase);
    }
}
