using Microsoft.AspNetCore.Components;
using RaphCare.Web.Services;

namespace RaphCare.Web.Layout;

public partial class AdminLayout
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IWebAuthService WebAuth { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        var token = await WebAuth.GetTokenAsync().ConfigureAwait(false);
        var kind = await WebAuth.GetAccountKindAsync().ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(token) || kind != WebAccountKind.Professional)
            Navigation.NavigateTo("/professional/signin", replace: true);
    }

    private async Task SignOutAsync()
    {
        await WebAuth.SignOutAsync().ConfigureAwait(false);
        Navigation.NavigateTo("/", replace: true);
    }
}
