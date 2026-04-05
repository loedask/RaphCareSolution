using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Profile;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

/// <summary>HTTP access to patient profile until operations are exposed on <see cref="IClient"/> (NSwag regen).</summary>
public sealed class PatientProfileService(IClient client, HttpClient httpClient) : BaseHttpService(client, httpClient), IPatientProfileService
{
    private const string ProfileUri = "api/patient/profile";

    public Task<Response<MyPatientProfileViewModel>> GetMyProfileAsync(CancellationToken cancellationToken = default) =>
        GetAsync<MyPatientProfileViewModel>(ProfileUri, cancellationToken);

    public Task<Response<bool>> UpdateMyProfileAsync(MyPatientProfileUpdateRequest request, CancellationToken cancellationToken = default) =>
        PutAsJsonNoContentAsync(ProfileUri, request, cancellationToken);
}
