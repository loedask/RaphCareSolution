using RaphCare.Client.Contracts;
using RaphCare.Mobile.Core.Shared.Models;

namespace RaphCare.Mobile.Core.Shared.Services.Auth;

/// <summary>
/// Handles Microsoft Entra ID sign-up, sign-in, and token storage for the mobile app.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Starts the Entra sign-up (registration) flow. For B2C/External ID this typically opens the sign-up policy.
    /// </summary>
    Task<AuthResult> SignUpWithEmailAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Signs in with Entra ID (interactive). Stores tokens in secure storage.
    /// </summary>
    Task<AuthResult> SignInAsync(CancellationToken cancellationToken = default);

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
