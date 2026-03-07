using RaphCare.Mobile.Core.Shared.Models;

namespace RaphCare.Mobile.Core.Shared.Services.Auth;

/// <summary>
/// Handles Microsoft Entra ID sign-up, sign-in, and token storage for the mobile app.
/// </summary>
public interface IAuthService
{
    Task<AuthResult> SignUpWithEmailAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<AuthResult> SignInAsync(CancellationToken cancellationToken = default);
    Task SignOutAsync(CancellationToken cancellationToken = default);
    Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    Task<bool> IsAuthenticatedAsync(CancellationToken cancellationToken = default);
}
