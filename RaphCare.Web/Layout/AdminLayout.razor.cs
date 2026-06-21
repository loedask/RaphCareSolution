using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using RaphCare.Web.Services;

namespace RaphCare.Web.Layout;

public partial class AdminLayout
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IWebAuthService WebAuth { get; set; } = default!;
    [Inject] private IClinicContextService ClinicContext { get; set; } = default!;

    private IReadOnlyList<(string Label, string Href, bool IsLast)> _breadcrumbs = [];
    private bool _clinicSwitcherOpen;

    protected override async Task OnInitializedAsync()
    {
        await WebAuth.EnsureHydratedAsync().ConfigureAwait(true);

        var token = await WebAuth.GetTokenAsync().ConfigureAwait(true);
        var kind = await WebAuth.GetAccountKindAsync().ConfigureAwait(true);
        var sessionValid = await WebAuth.IsSessionValidAsync().ConfigureAwait(true);
        if (string.IsNullOrWhiteSpace(token) || kind != WebAccountKind.Professional || !sessionValid)
        {
            var returnUrl = Uri.EscapeDataString(Navigation.Uri);
            Navigation.NavigateTo($"/professional/signin?returnUrl={returnUrl}", replace: true);
            return;
        }

        ClinicContext.Changed += OnClinicContextChanged;
        await ClinicContext.InitializeAsync().ConfigureAwait(true);

        Navigation.LocationChanged += OnLocationChanged;
        UpdateBreadcrumbs(Navigation.Uri);
    }

    private void OnClinicContextChanged() => _ = InvokeAsync(StateHasChanged);

    private void ToggleClinicSwitcher() => _clinicSwitcherOpen = !_clinicSwitcherOpen;

    private async Task SwitchClinicAsync(Guid clinicId, string name)
    {
        _clinicSwitcherOpen = false;
        await ClinicContext.SetCurrentClinicAsync(clinicId, name).ConfigureAwait(true);

        var path = new Uri(Navigation.Uri).AbsolutePath.TrimEnd('/');
        if (path.StartsWith("/admin/hospitals/", StringComparison.Ordinal)
            && path != "/admin/hospitals/register"
            && Guid.TryParse(path["/admin/hospitals/".Length..], out _))
        {
            Navigation.NavigateTo($"/admin/hospitals/{clinicId}");
        }
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
            _ when path.StartsWith("/admin/hospitals/", StringComparison.Ordinal) && path.EndsWith("/appointments", StringComparison.Ordinal) =>
            [
                ("RaphCare", "/admin", false),
                ("Hospitals", "/admin/hospitals", false),
                ("Appointments", path, true)
            ],
            _ when path.StartsWith("/admin/hospitals/", StringComparison.Ordinal) && path.Contains("/providers/", StringComparison.Ordinal) =>
            [
                ("RaphCare", "/admin", false),
                ("Hospitals", "/admin/hospitals", false),
                ("Provider", path, true)
            ],
            _ when path.StartsWith("/admin/hospitals/", StringComparison.Ordinal) && path != "/admin/hospitals/register" =>
            [
                ("RaphCare", "/admin", false),
                ("Hospitals", "/admin/hospitals", false),
                ("Hospital details", path, true)
            ],
            _ => [("RaphCare", "/admin", false), ("Clinic portal", "/admin", true)]
        };
    }

    private async Task SignOutAsync()
    {
        await WebAuth.SignOutAsync().ConfigureAwait(false);
        Navigation.NavigateTo("/", replace: true);
    }

    public void Dispose()
    {
        Navigation.LocationChanged -= OnLocationChanged;
        ClinicContext.Changed -= OnClinicContextChanged;
    }
}
