using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using RaphCare.Mobile.Core.Shared.Models;

namespace RaphCare.Mobile.Core.Shared.Services.Auth;

/// <summary>
/// Microsoft Entra ID authentication using MSAL. Handles sign-up, sign-in, and secure token storage.
/// </summary>
public class EntraAuthService : IAuthService
{
    private const string AccessTokenKey = "access_token";
    private const string ExpiresOnKey = "expires_on";

    private readonly IPublicClientApplication _msalClient;
    private readonly EntraAuthOptions _options;
    private readonly ILogger<EntraAuthService> _logger;

    public EntraAuthService(IOptions<EntraAuthOptions> options, ILogger<EntraAuthService> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _msalClient = BuildMsalClient();
    }

    private IPublicClientApplication BuildMsalClient()
    {
        var builder = PublicClientApplicationBuilder
            .Create(_options.ClientId)
            .WithAuthority(_options.GetAuthority())
            .WithRedirectUri(_options.GetRedirectUri());
        return builder.Build();
    }

    public async Task<AuthResult> SignUpWithEmailAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            var scopes = new[] { _options.ApiScope };
            var result = await _msalClient.AcquireTokenInteractive(scopes).ExecuteAsync(cancellationToken).ConfigureAwait(false);
            await StoreTokensAsync(result).ConfigureAwait(false);
            return AuthResult.Ok(result.AccessToken, null, result.ExpiresOn);
        }
        catch (MsalException ex)
        {
            _logger.LogWarning(ex, "Entra sign-up failed for {Email}", email);
            return AuthResult.Fail(ex.Message);
        }
    }

    public async Task<AuthResult> SignInAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var scopes = new[] { _options.ApiScope };
            var result = await _msalClient.AcquireTokenInteractive(scopes).ExecuteAsync(cancellationToken).ConfigureAwait(false);
            await StoreTokensAsync(result).ConfigureAwait(false);
            return AuthResult.Ok(result.AccessToken, null, result.ExpiresOn);
        }
        catch (MsalException ex)
        {
            _logger.LogWarning(ex, "Entra sign-in failed");
            return AuthResult.Fail(ex.Message);
        }
    }

    public async Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        var accounts = await _msalClient.GetAccountsAsync().ConfigureAwait(false);
        foreach (var account in accounts)
            await _msalClient.RemoveAsync(account).ConfigureAwait(false);
        SecureStorage.Default.Remove(AccessTokenKey);
        SecureStorage.Default.Remove(ExpiresOnKey);
    }

    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var token = await SecureStorage.Default.GetAsync(AccessTokenKey).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(token))
        {
            var expiresOnStr = await SecureStorage.Default.GetAsync(ExpiresOnKey).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(expiresOnStr) && DateTimeOffset.TryParse(expiresOnStr, out var expiresOn) && expiresOn > DateTimeOffset.UtcNow.AddMinutes(5))
                return token;
        }
        try
        {
            var accounts = (await _msalClient.GetAccountsAsync().ConfigureAwait(false)).ToList();
            if (accounts.Count == 0) return null;
            var result = await _msalClient.AcquireTokenSilent(new[] { _options.ApiScope }, accounts[0]).ExecuteAsync(cancellationToken).ConfigureAwait(false);
            await StoreTokensAsync(result).ConfigureAwait(false);
            return result.AccessToken;
        }
        catch (MsalUiRequiredException) { return null; }
    }

    public async Task<bool> IsAuthenticatedAsync(CancellationToken cancellationToken = default)
    {
        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        return !string.IsNullOrEmpty(token);
    }

    private static async Task StoreTokensAsync(AuthenticationResult result)
    {
        await SecureStorage.Default.SetAsync(AccessTokenKey, result.AccessToken).ConfigureAwait(false);
        await SecureStorage.Default.SetAsync(ExpiresOnKey, result.ExpiresOn.ToString("O")).ConfigureAwait(false);
    }
}
