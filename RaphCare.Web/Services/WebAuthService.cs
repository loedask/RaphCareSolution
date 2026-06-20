using Microsoft.JSInterop;

namespace RaphCare.Web.Services;

public interface IWebAuthService
{
    Task<WebAccountKind> GetAccountKindAsync();
    Task<string?> GetTokenAsync();
    Task<bool> IsSessionValidAsync();
    Task StoreSessionAsync(string token, WebAccountKind kind);
    Task SignOutAsync();
}

public sealed class WebAuthService(IJSRuntime js) : IWebAuthService
{
    public async Task<WebAccountKind> GetAccountKindAsync()
    {
        var kind = await js.InvokeAsync<string?>("raphCareAuth.getAccountKind").ConfigureAwait(false);
        return kind switch
        {
            "patient" => WebAccountKind.Patient,
            "professional" => WebAccountKind.Professional,
            _ => WebAccountKind.None
        };
    }

    public Task<string?> GetTokenAsync() =>
        js.InvokeAsync<string?>("raphCareAuth.getToken").AsTask();

    public Task<bool> IsSessionValidAsync() =>
        js.InvokeAsync<bool>("raphCareAuth.isSessionValid").AsTask();

    public Task StoreSessionAsync(string token, WebAccountKind kind)
    {
        var kindValue = kind == WebAccountKind.Patient ? "patient" : "professional";
        return js.InvokeVoidAsync("raphCareAuth.setToken", token, kindValue).AsTask();
    }

    public Task SignOutAsync() => js.InvokeVoidAsync("raphCareAuth.clear").AsTask();
}
