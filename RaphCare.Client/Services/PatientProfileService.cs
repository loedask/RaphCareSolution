using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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

    public async Task<Response<string>> UploadProfilePhotoAsync(
        Stream photoStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (photoStream is null)
            return Response<string>.Failure("Photo is required.");

        try
        {
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(photoStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(contentType) ? "image/jpeg" : contentType.Trim());
            content.Add(streamContent, "photo", string.IsNullOrWhiteSpace(fileName) ? "photo.jpg" : fileName.Trim());

            using var response = await HttpClient.PostAsync("api/patient/profile/photo", content, cancellationToken)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                return Response<string>.Failure(
                    string.IsNullOrEmpty(err) ? $"API error: {response.StatusCode}" : err,
                    (int)response.StatusCode);
            }

            var result = await response.Content.ReadFromJsonAsync<UploadPhotoDto>(ApiJson.Options, cancellationToken)
                .ConfigureAwait(false);
            return string.IsNullOrWhiteSpace(result?.PhotoUrl)
                ? Response<string>.Failure("Upload succeeded but no photo URL was returned.")
                : Response<string>.Success(result.PhotoUrl);
        }
        catch (HttpRequestException)
        {
            return Response<string>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<byte[]?>> GetProfilePhotoBytesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await HttpClient.GetAsync("api/patient/profile/photo", cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return Response<byte[]?>.Success(null);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                return Response<byte[]?>.Failure(
                    string.IsNullOrEmpty(err) ? $"API error: {response.StatusCode}" : err,
                    (int)response.StatusCode);
            }

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            return Response<byte[]?>.Success(bytes);
        }
        catch (HttpRequestException)
        {
            return Response<byte[]?>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
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
            ProfilePhotoUrl = d.ProfilePhotoUrl
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
        public string? ProfilePhotoUrl { get; set; }
    }

    private sealed class UploadPhotoDto
    {
        public string? PhotoUrl { get; set; }
    }
}
