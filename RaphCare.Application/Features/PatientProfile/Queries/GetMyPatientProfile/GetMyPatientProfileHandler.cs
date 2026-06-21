using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientProfile.DTOs;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.PatientProfile.Queries.GetMyPatientProfile;

public sealed class GetMyPatientProfileHandler(
    IRepository<Patient> patients,
    IRepository<Domain.Patients.PatientProfile> profiles,
    ICurrentUserService currentUser) : IRequestHandler<GetMyPatientProfileQuery, MyPatientProfileDto>
{
    private readonly IRepository<Patient> _patients = patients;
    private readonly IRepository<Domain.Patients.PatientProfile> _profiles = profiles;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<MyPatientProfileDto> Handle(GetMyPatientProfileQuery request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var patient = await _patients.GetByIdAsync(patientId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException(nameof(Patient), patientId);

        var profilePage = await _profiles.SearchAsync(
            q => q.Where(p => p.PatientId == patientId),
            1,
            1,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var profile = profilePage.Items.Count > 0 ? profilePage.Items[0] : null;
        string? photoUrl = !string.IsNullOrWhiteSpace(profile?.ProfilePhotoRelativePath)
            ? PatientProfilePhotoUrls.RelativePhotoPath
            : null;

        return new MyPatientProfileDto
        {
            PatientId = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            Email = patient.Email,
            PhoneNumber = patient.PhoneNumber,
            DateOfBirth = patient.DateOfBirth,
            Gender = patient.Gender,
            ProfilePhotoUrl = photoUrl
        };
    }
}
