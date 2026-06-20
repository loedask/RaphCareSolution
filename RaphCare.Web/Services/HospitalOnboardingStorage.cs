using System.Text.Json;
using Microsoft.JSInterop;

namespace RaphCare.Web.Services;

public interface IHospitalOnboardingStorage
{
    Task SaveAsync(HospitalOnboardingDraft draft, CancellationToken cancellationToken = default);
    Task<HospitalOnboardingDraft?> LoadAsync(CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
}

public sealed class HospitalOnboardingStorage(IJSRuntime js) : IHospitalOnboardingStorage
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task SaveAsync(HospitalOnboardingDraft draft, CancellationToken cancellationToken = default)
    {
        draft.SavedAt = DateTimeOffset.UtcNow;
        var json = JsonSerializer.Serialize(draft, JsonOptions);
        await js.InvokeVoidAsync("raphCareOnboarding.save", cancellationToken, json).ConfigureAwait(false);
    }

    public async Task<HospitalOnboardingDraft?> LoadAsync(CancellationToken cancellationToken = default)
    {
        var json = await js.InvokeAsync<string?>("raphCareOnboarding.load", cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<HospitalOnboardingDraft>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public Task ClearAsync(CancellationToken cancellationToken = default) =>
        js.InvokeVoidAsync("raphCareOnboarding.clear", cancellationToken).AsTask();
}
