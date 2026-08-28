using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.PatientProfile.Queries.GetMyPatientProfilePhoto;

public sealed class GetMyPatientProfilePhotoQuery : IRequest<PatientProfilePhotoReadResult?>;

public sealed class GetMyPatientProfilePhotoHandler(
    IPatientProfilePhotoStorage photoStorage,
    ICurrentUserService currentUser) : IRequestHandler<GetMyPatientProfilePhotoQuery, PatientProfilePhotoReadResult?>
{
    private readonly IPatientProfilePhotoStorage _photoStorage = photoStorage;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<PatientProfilePhotoReadResult?> Handle(
        GetMyPatientProfilePhotoQuery request,
        CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        return await _photoStorage.OpenReadAsync(patientId, cancellationToken).ConfigureAwait(false);
    }
}
