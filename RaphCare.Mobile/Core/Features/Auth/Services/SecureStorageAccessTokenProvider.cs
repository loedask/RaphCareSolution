using RaphCare.Client.Contracts;

namespace RaphCare.Mobile.Core.Features.Auth.Services;

/// <summary>
/// Provides the current access token for API requests. Uses <see cref="IAuthService"/> which reads from secure storage and handles silent refresh.
/// </summary>
public class SecureStorageAccessTokenProvider(IAuthService authService) : IAccessTokenProvider
{
    private readonly IAuthService _authService = authService ?? throw new ArgumentNullException(nameof(authService));

    public Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default) =>
        _authService.GetAccessTokenAsync(cancellationToken);
}
