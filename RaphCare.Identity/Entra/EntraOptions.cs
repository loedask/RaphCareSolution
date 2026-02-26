namespace RaphCare.Identity.Entra;

/// <summary>
/// Strongly-typed configuration for Microsoft Entra ID (Azure AD) integration.
/// Bind from configuration section, e.g. "Entra" or "AzureAd".
/// </summary>
public class EntraOptions
{
    public const string SectionName = "Entra";

    public string Authority { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string[] ValidIssuers { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Builds the full authority URL from TenantId if Authority is not set.
    /// </summary>
    public string GetAuthority() =>
        !string.IsNullOrWhiteSpace(Authority)
            ? Authority.TrimEnd('/')
            : $"https://login.microsoftonline.com/{TenantId}/v2.0";
}
