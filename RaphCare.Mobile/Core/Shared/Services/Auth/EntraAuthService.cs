using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Mobile.Core.Shared.Models;

namespace RaphCare.Mobile.Core.Shared.Services.Auth;

/// <summary>
/// Microsoft Entra ID authentication using MSAL. Handles sign-up, sign-in, and secure token storage.
/// </summary>
public class EntraAuthService : IAuthService
{
    private const string AccessTokenKey = "access_token";
    private const string ExpiresOnKey = "expires_on";

    private readonly IPublicClientApplication? _msalClient;
    private readonly EntraAuthOptions _options;
    private readonly ILogger<EntraAuthService> _logger;
    private readonly IEmailAuthService _emailAuthService;

    public EntraAuthService(IOptions<EntraAuthOptions> options, ILogger<EntraAuthService> logger, IEmailAuthService emailAuthService)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _emailAuthService = emailAuthService ?? throw new ArgumentNullException(nameof(emailAuthService));

        if (string.IsNullOrWhiteSpace(_options.ClientId))
        {
            _logger.LogCritical("Entra:ClientId is missing. Entra sign-in/up is disabled.");
            return;
        }

        try
        {
            _msalClient = BuildMsalClient();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "MSAL failed to initialize. Entra sign-in/up is disabled.");
        }
    }

    private IPublicClientApplication BuildMsalClient()
    {
        var builder = PublicClientApplicationBuilder
            .Create(_options.ClientId.Trim())
            .WithAuthority(_options.GetAuthority())
            .WithRedirectUri(_options.GetRedirectUri());

#if ANDROID
        // Required for interactive auth on Android; avoids native/UI-thread failures during token acquisition.
        builder = builder.WithParentActivityOrWindow(() =>
            Microsoft.Maui.ApplicationModel.Platform.CurrentActivity);
#endif

        return builder.Build();
    }

    private static AuthResult MsalUnavailable() =>
        AuthResult.Fail("Sign-in is not available. Check Microsoft Entra configuration in appsettings.");

    public async Task<AuthResult> RegisterWithEmailAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        Guid? clinicId = null,
        string? verificationCode = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _emailAuthService
            .RegisterAsync(firstName, lastName, email, password, clinicId, verificationCode, cancellationToken)
            .ConfigureAwait(false);
        if (!response.IsSuccess || response.Data?.Success != true || string.IsNullOrWhiteSpace(response.Data.Token))
            return AuthResult.Fail(response.ErrorMessage ?? "Registration failed.");

        var expiresOn = DateTimeOffset.UtcNow.AddHours(12);
        await StoreTokensFromApiAsync(response.Data.Token, expiresOn).ConfigureAwait(false);
        return AuthResult.Ok(response.Data.Token, null, expiresOn);
    }

    public async Task<AuthResult> SignInWithEmailAsync(
        string email,
        string password,
        string? verificationCode = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _emailAuthService
            .SignInAsync(email, password, verificationCode, cancellationToken)
            .ConfigureAwait(false);
        if (!response.IsSuccess || response.Data is null)
            return AuthResult.Fail(response.ErrorMessage ?? "Sign-in failed.");

        if (response.Data.RequiresVerification)
            return AuthResult.PendingVerification();

        if (!response.Data.Success || string.IsNullOrWhiteSpace(response.Data.Token))
            return AuthResult.Fail(response.ErrorMessage ?? "Sign-in failed.");

        var expiresOn = DateTimeOffset.UtcNow.AddHours(12);
        await StoreTokensFromApiAsync(response.Data.Token, expiresOn).ConfigureAwait(false);
        return AuthResult.Ok(response.Data.Token, null, expiresOn);
    }

    public async Task<AuthResult> SignUpWithEmailAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (_msalClient is null)
            return MsalUnavailable();

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
        if (_msalClient is null)
            return MsalUnavailable();

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

    /// <inheritdoc />
    public async Task<AuthResult> AcquireTokenInteractiveAsync(string authority, IReadOnlyList<string> scopes, CancellationToken cancellationToken = default)
    {
        if (_msalClient is null)
            return MsalUnavailable();

        if (string.IsNullOrWhiteSpace(authority))
            return AuthResult.Fail("Authority is required.");

        var scopeArray = scopes is { Count: > 0 }
            ? scopes.ToArray()
            : new[] { "openid" };

        try
        {
            var authorityString = authority.Trim();
#pragma warning disable CS0618 // B2C per-policy authority; string WithAuthority still required for user-flow hosts until we adopt a single CIAM authority model.
            var builder = _msalClient
                .AcquireTokenInteractive(scopeArray)
                .WithAuthority(authorityString);
#pragma warning restore CS0618
            var result = await builder
                .ExecuteAsync(cancellationToken)
                .ConfigureAwait(false);
            await StoreTokensAsync(result).ConfigureAwait(false);
            return AuthResult.Ok(result.AccessToken, null, result.ExpiresOn);
        }
        catch (MsalException ex)
        {
            _logger.LogWarning(ex, "Interactive acquire failed for authority {Authority}", authority);
            return AuthResult.Fail(ex.Message);
        }
    }

    /// <inheritdoc />
    public Task StoreApiSessionAsync(string accessToken, DateTimeOffset expiresOnUtc, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return Task.CompletedTask;
        return StoreTokensFromApiAsync(accessToken, expiresOnUtc);
    }

    private static async Task StoreTokensFromApiAsync(string accessToken, DateTimeOffset expiresOnUtc)
    {
        await SecureStorage.Default.SetAsync(AccessTokenKey, accessToken).ConfigureAwait(false);
        await SecureStorage.Default.SetAsync(ExpiresOnKey, expiresOnUtc.ToString("O")).ConfigureAwait(false);
    }

    public async Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        if (_msalClient is not null)
        {
            var accounts = await _msalClient.GetAccountsAsync().ConfigureAwait(false);
            foreach (var account in accounts)
                await _msalClient.RemoveAsync(account).ConfigureAwait(false);
        }

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

        if (_msalClient is null)
            return null;

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
