namespace RaphCare.Identity.LocalJwt;

/// <summary>Configuration for API-issued HS256 JWTs (email/OTP patients and staff).</summary>
public sealed class LocalJwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
}
