namespace RaphCare.Mobile.Core.Shared.Models;

/// <summary>
/// Result of an authentication operation (sign-up or sign-in).
/// </summary>
public class AuthResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTimeOffset? ExpiresOn { get; set; }

    public static AuthResult Ok(string? accessToken = null, string? refreshToken = null, DateTimeOffset? expiresOn = null) =>
        new() { Success = true, AccessToken = accessToken, RefreshToken = refreshToken, ExpiresOn = expiresOn };

    public static AuthResult Fail(string message) =>
        new() { Success = false, ErrorMessage = message };
}
