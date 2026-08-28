using Microsoft.Maui.Devices;

namespace RaphCare.Mobile.Core.Common.Services.Auth;

/// <summary>
/// Configuration for Microsoft Entra ID (B2C / External ID) used by the mobile app.
/// </summary>
/// <remarks>
/// <para><b>B2C password reset (browser):</b> paste the user-flow "Run now" HTTPS URL into <see cref="SelfServicePasswordResetUrl"/>.</para>
/// <para><b>B2C password reset (native MSAL):</b> set <see cref="B2CPasswordResetAuthority"/> to the policy authority, e.g.
/// <c>https://{tenant}.b2clogin.com/{tenant}.onmicrosoft.com/B2C_1A_PWDRESET</c> (no trailing slash; same shape as your sign-in policy host).</para>
/// <para><b>B2C sign-up (browser):</b> paste the sign-up user flow "Run now" URL into <see cref="ExternalSignUpUrl"/>. When set, Create Account opens the browser instead of in-app registration.</para>
/// </remarks>
public class EntraAuthOptions
{
    public const string SectionName = "Entra";

    public string ClientId { get; set; } = string.Empty;
    public string TenantId { get; set; } = "common";
    public string? Authority { get; set; }
    public string RedirectUri { get; set; } = "msal{ClientId}://auth";
    public string ApiScope { get; set; } = "api://raphcare-api/.default";

    /// <summary>Optional. Opened when the user taps "Forgot password?" if <see cref="B2CPasswordResetAuthority"/> is not set (workforce SSPR, or B2C user-flow URL from Azure portal).</summary>
    public string? SelfServicePasswordResetUrl { get; set; }

    /// <summary>Optional. B2C password reset policy authority for an in-app MSAL interactive reset. If set, it takes precedence over <see cref="SelfServicePasswordResetUrl"/>.</summary>
    public string? B2CPasswordResetAuthority { get; set; }

    /// <summary>Space-separated scopes for the B2C password reset step. Default <c>openid</c>.</summary>
    public string B2CPasswordResetScopes { get; set; } = "openid";

    /// <summary>Optional. Full HTTPS URL to start B2C (or other) sign-up in the system browser. When non-empty, Create Account uses this instead of navigating to registration pages.</summary>
    public string? ExternalSignUpUrl { get; set; }

    public string[] GetPasswordResetScopes() =>
        B2CPasswordResetScopes.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

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
