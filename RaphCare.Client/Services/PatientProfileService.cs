using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Profile;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public sealed class PatientProfileService(HttpClient httpClient) : BaseHttpService(httpClient), IPatientProfileService
{
    public async Task<Response<MyPatientProfileViewModel>> GetMyProfileAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<ProfileDto>("api/patient/profile", cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<MyPatientProfileViewModel>.Failure(result.ErrorMessage ?? "Could not load profile.", result.StatusCode);
        return Response<MyPatientProfileViewModel>.Success(Map(result.Data));
    }

    public async Task<Response<bool>> UpdateMyProfileAsync(MyPatientProfileUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var result = await PutNoContentAsync("api/patient/profile", request, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess
            ? Response<bool>.Success(true)
            : Response<bool>.Failure(result.ErrorMessage ?? "Update failed.", result.StatusCode);
    }

    private static MyPatientProfileViewModel Map(ProfileDto d) =>
        new()
        {
            PatientId = d.PatientId,
            FirstName = d.FirstName ?? string.Empty,
            LastName = d.LastName ?? string.Empty,
            Email = d.Email,
            PhoneNumber = d.PhoneNumber,
            DateOfBirth = d.DateOfBirth,
            Gender = d.Gender ?? string.Empty,
        };

    private sealed class ProfileDto
    {
        public Guid PatientId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Gender { get; set; }
    }
}
