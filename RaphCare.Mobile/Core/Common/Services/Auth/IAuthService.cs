using RaphCare.Client.Contracts;
using RaphCare.Mobile.Core.Shared.Models;

namespace RaphCare.Mobile.Core.Shared.Services.Auth;

/// <summary>
/// Handles Microsoft Entra ID sign-up, sign-in, and token storage for the mobile app.
/// </summary>
public interface IAuthService
{
    Task<AuthResult> RegisterWithEmailAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        Guid? clinicId = null,
        string? verificationCode = null,
        CancellationToken cancellationToken = default);

    Task<AuthResult> SignInWithEmailAsync(
        string email,
        string password,
        string? verificationCode = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts the Entra sign-up (registration) flow. For B2C/External ID this typically opens the sign-up policy.
    /// </summary>
    Task<AuthResult> SignUpWithEmailAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Signs in with Entra ID (interactive). Stores tokens in secure storage.
    /// </summary>
    Task<AuthResult> SignInAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Interactive token acquisition using a specific authority (e.g. B2C password reset policy). Stores tokens on success.
    /// </summary>
    Task<AuthResult> AcquireTokenInteractiveAsync(string authority, IReadOnlyList<string> scopes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists an API-issued JWT (e.g. after OTP verify). Same storage as Entra tokens for <see cref="IAccessTokenProvider"/>.
    /// </summary>
    Task StoreApiSessionAsync(string accessToken, DateTimeOffset expiresOnUtc, CancellationToken cancellationToken = default);

    /// <summary>
    /// Signs out and clears stored tokens.
    /// </summary>
    Task SignOutAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the current access token if available. Used by <see cref="IAccessTokenProvider"/> for API requests.
    /// </summary>
    Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// True if the user has a valid stored session (token present and not expired).
    /// </summary>
    Task<bool> IsAuthenticatedAsync(CancellationToken cancellationToken = default);
}
