using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models;
using RaphCare.Portal.Resources.Strings;
using RaphCare.Portal.Services;
using RaphCare.Portal.Services.Localization;
using RaphCare.Ui.Components;

namespace RaphCare.Portal.Layout;

public partial class AdminLayout
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IWebAuthService WebAuth { get; set; } = default!;
    [Inject] private IClinicContextService ClinicContext { get; set; } = default!;
    [Inject] private IAdminClinicService AdminClinicService { get; set; } = default!;
    [Inject] private IUiCultureService CultureService { get; set; } = default!;

    private static SearchableSelectLabels SelectLabels => new()
    {
        Search = AppResources.T("Common_Search", CultureInfo.CurrentUICulture),
        NoMatchingOptions = AppResources.T("Common_NoMatchingOptions", CultureInfo.CurrentUICulture)
    };

    private bool _clinicSwitcherOpen;
    private string _searchQuery = string.Empty;
    private bool _searchOpen;
    private bool _searchingPatients;
    private CancellationTokenSource? _patientSearchCts;
    private IReadOnlyList<ClinicListItem> _hospitalResults = Array.Empty<ClinicListItem>();
    private IReadOnlyList<ClinicPatientListItem> _patientResults = Array.Empty<ClinicPatientListItem>();

    private string DashboardHref =>
        ClinicContext.CurrentClinicId is not null
            ? "/admin"
            : "/admin/hospitals";

    protected override async Task OnInitializedAsync()
    {
        CultureService.CultureChanged += OnCultureChanged;

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
    }

    private void OnCultureChanged() => _ = InvokeAsync(StateHasChanged);

    private void OnClinicContextChanged() => _ = InvokeAsync(StateHasChanged);

    private void ToggleClinicSwitcher() => _clinicSwitcherOpen = !_clinicSwitcherOpen;

    private async Task OnSearchInputAsync(ChangeEventArgs e)
    {
        _searchQuery = e.Value?.ToString() ?? string.Empty;
        _clinicSwitcherOpen = false;
        UpdateHospitalResults();

        _patientSearchCts?.Cancel();
        _patientSearchCts?.Dispose();
        _patientSearchCts = null;
        _patientResults = Array.Empty<ClinicPatientListItem>();

        var q = _searchQuery.Trim();
        _searchOpen = q.Length > 0;

        if (q.Length < 2 || ClinicContext.CurrentClinicId is not Guid clinicId)
        {
            _searchingPatients = false;
            return;
        }

        var cts = new CancellationTokenSource();
        _patientSearchCts = cts;
        _searchingPatients = true;
        await InvokeAsync(StateHasChanged);

        try
        {
            await Task.Delay(250, cts.Token).ConfigureAwait(true);
            var response = await AdminClinicService.GetPatientsAsync(clinicId, 1, 8, q, cts.Token).ConfigureAwait(true);
            if (cts.IsCancellationRequested)
                return;

            _patientResults = response.IsSuccess && response.Data is not null
                ? response.Data.Items
                : Array.Empty<ClinicPatientListItem>();
        }
        catch (OperationCanceledException)
        {
            return;
        }
        finally
        {
            if (_patientSearchCts == cts)
            {
                _searchingPatients = false;
                _patientSearchCts = null;
            }
        }
    }

    private void UpdateHospitalResults()
    {
        var q = _searchQuery.Trim();
        if (q.Length == 0)
        {
            _hospitalResults = Array.Empty<ClinicListItem>();
            return;
        }

        _hospitalResults = ClinicContext.MyClinics
            .Where(c =>
                c.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrWhiteSpace(c.ReferenceCode)
                    && c.ReferenceCode.Contains(q, StringComparison.OrdinalIgnoreCase))
                || (!string.IsNullOrWhiteSpace(c.RegistrationNumber)
                    && c.RegistrationNumber.Contains(q, StringComparison.OrdinalIgnoreCase)))
            .Take(8)
            .ToList();
    }

    private void OnSearchKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            _searchOpen = false;
            _searchQuery = string.Empty;
            _hospitalResults = Array.Empty<ClinicListItem>();
            _patientResults = Array.Empty<ClinicPatientListItem>();
        }
    }

    private async Task NavigateToHospitalAsync(Guid clinicId, string name)
    {
        _searchOpen = false;
        _searchQuery = string.Empty;
        _hospitalResults = Array.Empty<ClinicListItem>();
        _patientResults = Array.Empty<ClinicPatientListItem>();
        await ClinicContext.SetCurrentClinicAsync(clinicId, name).ConfigureAwait(true);
        Navigation.NavigateTo($"/admin/hospitals/{clinicId}");
    }

    private void NavigateToPatient(Guid patientId)
    {
        if (ClinicContext.CurrentClinicId is not Guid clinicId)
            return;

        _searchOpen = false;
        _searchQuery = string.Empty;
        _hospitalResults = Array.Empty<ClinicListItem>();
        _patientResults = Array.Empty<ClinicPatientListItem>();
        Navigation.NavigateTo($"/admin/hospitals/{clinicId}/patients/{patientId}");
    }

    private async Task SwitchClinicAsync(Guid clinicId, string name)
    {
        _clinicSwitcherOpen = false;
        await ClinicContext.SetCurrentClinicAsync(clinicId, name).ConfigureAwait(true);
        Navigation.NavigateTo($"/admin/hospitals/{clinicId}");
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        _searchOpen = false;
        _clinicSwitcherOpen = false;
        InvokeAsync(StateHasChanged);
    }

    private async Task SignOutAsync()
    {
        await WebAuth.SignOutAsync().ConfigureAwait(false);
        Navigation.NavigateTo("/", replace: true);
    }

    public void Dispose()
    {
        CultureService.CultureChanged -= OnCultureChanged;
        Navigation.LocationChanged -= OnLocationChanged;
        ClinicContext.Changed -= OnClinicContextChanged;
        _patientSearchCts?.Cancel();
        _patientSearchCts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
