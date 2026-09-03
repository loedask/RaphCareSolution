using RaphCare.Client.Contracts;
using RaphCare.Portal.Services;

namespace RaphCare.Portal.Services;

public sealed class BrowserAccessTokenProvider(IWebAuthService webAuth) : IAccessTokenProvider
{
    public Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default) =>
        webAuth.GetTokenAsync();
}
