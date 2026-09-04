using Microsoft.JSInterop;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models;

namespace RaphCare.Portal.Services;

public interface IClinicContextService
{
    Guid? CurrentClinicId { get; }
    string? CurrentClinicName { get; }
    IReadOnlyList<ClinicListItem> MyClinics { get; }
    event Action? Changed;
    Task InitializeAsync();
    Task SetCurrentClinicAsync(Guid clinicId, string? name = null);
    Task TryClaimPendingClinicAsync();
}

public sealed class ClinicContextService(
    IAdminClinicService adminClinicService,
    WebActiveClinicIdStore activeClinicId,
    IJSRuntime js) : IClinicContextService
{
    private bool _initialized;

    public event Action? Changed;

    public Guid? CurrentClinicId { get; private set; }
    public string? CurrentClinicName { get; private set; }
    public IReadOnlyList<ClinicListItem> MyClinics { get; private set; } = Array.Empty<ClinicListItem>();

    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        await TryClaimPendingClinicAsync().ConfigureAwait(true);

        var response = await adminClinicService.GetClinicsAsync().ConfigureAwait(true);
        if (response.IsSuccess && response.Data is not null)
            MyClinics = response.Data;

        var storedId = await js.InvokeAsync<string?>("raphCareClinic.getClinicId").ConfigureAwait(true);
        if (Guid.TryParse(storedId, out var clinicId))
        {
            var match = MyClinics.FirstOrDefault(c => c.Id == clinicId);
            if (match is not null)
            {
                CurrentClinicId = clinicId;
                CurrentClinicName = match.Name;
                activeClinicId.ClinicId = clinicId;
            }
        }

        if (CurrentClinicId is null && MyClinics.Count == 1)
            await SetCurrentClinicAsync(MyClinics[0].Id, MyClinics[0].Name).ConfigureAwait(true);
        else if (CurrentClinicId is null)
            activeClinicId.ClinicId = null;

        _initialized = true;
        Changed?.Invoke();
    }

    public async Task SetCurrentClinicAsync(Guid clinicId, string? name = null)
    {
        CurrentClinicId = clinicId;
        CurrentClinicName = name ?? MyClinics.FirstOrDefault(c => c.Id == clinicId)?.Name;
        activeClinicId.ClinicId = clinicId;
        await js.InvokeVoidAsync("raphCareClinic.setClinicId", clinicId.ToString()).ConfigureAwait(true);
        Changed?.Invoke();
    }

    public async Task TryClaimPendingClinicAsync()
    {
        var pending = await js.InvokeAsync<string?>("raphCareClinic.getPendingClinicId").ConfigureAwait(true);
        if (!Guid.TryParse(pending, out var clinicId))
            return;

        var claim = await adminClinicService.EnsureMembershipAsync(clinicId).ConfigureAwait(true);
        if (claim.IsSuccess)
            await js.InvokeVoidAsync("raphCareClinic.clearPendingClinicId").ConfigureAwait(true);
    }
}
