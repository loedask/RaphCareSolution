using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.PatientProfile.Commands.UploadMyPatientProfilePhoto;

public sealed class UploadMyPatientProfilePhotoHandler(
    IRepository<Patient> patients,
    IRepository<Domain.Patients.PatientProfile> profiles,
    IPatientProfilePhotoStorage photoStorage,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser) : IRequestHandler<UploadMyPatientProfilePhotoCommand, string>
{
    private readonly IRepository<Patient> _patients = patients;
    private readonly IRepository<Domain.Patients.PatientProfile> _profiles = profiles;
    private readonly IPatientProfilePhotoStorage _photoStorage = photoStorage;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<string> Handle(UploadMyPatientProfilePhotoCommand request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        _ = await _patients.GetByIdAsync(patientId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException(nameof(Patient), patientId);

        var saved = await _photoStorage.SaveAsync(patientId, request.Content, request.ContentType, cancellationToken)
            .ConfigureAwait(false);

        var page = await _profiles.SearchAsync(
            q => q.Where(p => p.PatientId == patientId),
            1,
            1,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var profile = page.Items.Count > 0 ? page.Items[0] : null;
        if (profile is null)
        {
            profile = new Domain.Patients.PatientProfile
            {
                PatientId = patientId,
                ProfilePhotoRelativePath = saved.RelativePath
            };
            await _profiles.AddAsync(profile, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            profile.ProfilePhotoRelativePath = saved.RelativePath;
            await _profiles.UpdateAsync(profile, cancellationToken).ConfigureAwait(false);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return PatientProfilePhotoUrls.RelativePhotoPath;
    }
}
