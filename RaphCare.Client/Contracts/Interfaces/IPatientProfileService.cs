using RaphCare.Client.Contracts;
using RaphCare.Client.Models.Profile;

namespace RaphCare.Client.Contracts.Interfaces;

/// <summary>Patient self-service profile (<c>api/patient/profile</c>).</summary>
public interface IPatientProfileService
{
    Task<Response<MyPatientProfileViewModel>> GetMyProfileAsync(CancellationToken cancellationToken = default);

    Task<Response<bool>> UpdateMyProfileAsync(MyPatientProfileUpdateRequest request, CancellationToken cancellationToken = default);

    Task<Response<string>> UploadProfilePhotoAsync(
        Stream photoStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<Response<byte[]?>> GetProfilePhotoBytesAsync(CancellationToken cancellationToken = default);
}
