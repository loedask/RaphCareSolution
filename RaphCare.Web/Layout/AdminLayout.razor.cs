using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using RaphCare.Web.Services;

namespace RaphCare.Web.Layout;

public partial class AdminLayout
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IWebAuthService WebAuth { get; set; } = default!;

    private IReadOnlyList<(string Label, string Href, bool IsLast)> _breadcrumbs = [];

    protected override async Task OnInitializedAsync()
    {
        var token = await WebAuth.GetTokenAsync().ConfigureAwait(false);
        var kind = await WebAuth.GetAccountKindAsync().ConfigureAwait(false);
        var sessionValid = await WebAuth.IsSessionValidAsync().ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(token) || kind != WebAccountKind.Professional || !sessionValid)
        {
            var returnUrl = Uri.EscapeDataString(Navigation.Uri);
            Navigation.NavigateTo($"/professional/signin?returnUrl={returnUrl}", replace: true);
        }

        Navigation.LocationChanged += OnLocationChanged;
        UpdateBreadcrumbs(Navigation.Uri);
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        UpdateBreadcrumbs(e.Location);
        InvokeAsync(StateHasChanged);
    }

    private void UpdateBreadcrumbs(string uri)
    {
        var path = new Uri(uri).AbsolutePath.TrimEnd('/');
        if (string.IsNullOrEmpty(path))
            path = "/";

        _breadcrumbs = path switch
        {
            "/admin" => [("RaphCare", "/admin", false), ("Dashboard", "/admin", true)],
            "/admin/hospitals" => [("RaphCare", "/admin", false), ("Hospitals", "/admin/hospitals", true)],
            "/admin/hospitals/register" =>
            [
                ("RaphCare", "/admin", false),
                ("Hospitals", "/admin/hospitals", false),
                ("Register hospital", "/admin/hospitals/register", true)
            ],
            _ => [("RaphCare", "/admin", false), ("Clinic portal", "/admin", true)]
        };
    }

    private async Task SignOutAsync()
    {
        await WebAuth.SignOutAsync().ConfigureAwait(false);
        Navigation.NavigateTo("/", replace: true);
    }

    public void Dispose() => Navigation.LocationChanged -= OnLocationChanged;
}
