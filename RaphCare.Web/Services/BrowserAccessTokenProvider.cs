using RaphCare.Client.Contracts;
using RaphCare.Web.Services;

namespace RaphCare.Web.Services;

public sealed class BrowserAccessTokenProvider(IWebAuthService webAuth) : IAccessTokenProvider
{
    public Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default) =>
        webAuth.GetTokenAsync();
}
