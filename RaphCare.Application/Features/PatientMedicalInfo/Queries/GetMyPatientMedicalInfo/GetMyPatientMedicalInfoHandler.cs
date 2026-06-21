using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientMedicalInfo.DTOs;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.PatientMedicalInfo.Queries.GetMyPatientMedicalInfo;

public sealed class GetMyPatientMedicalInfoHandler(
    IRepository<Domain.Patients.PatientProfile> profiles,
    ICurrentUserService currentUser) : IRequestHandler<GetMyPatientMedicalInfoQuery, MyPatientMedicalInfoDto>
{
    private readonly IRepository<Domain.Patients.PatientProfile> _profiles = profiles;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<MyPatientMedicalInfoDto> Handle(GetMyPatientMedicalInfoQuery request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var page = await _profiles.SearchAsync(
            q => q.Where(p => p.PatientId == patientId),
            1,
            1,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var profile = page.Items.FirstOrDefault();
        if (profile is null)
            return new MyPatientMedicalInfoDto();

        return new MyPatientMedicalInfoDto
        {
            BloodType = profile.BloodType ?? string.Empty,
            Allergies = profile.SelfReportedAllergies ?? string.Empty,
            ChronicConditions = profile.SelfReportedChronicConditions ?? string.Empty,
            Medications = profile.SelfReportedMedications ?? string.Empty,
            PrimaryDoctor = profile.PrimaryCareProviderName ?? string.Empty,
        };
    }
}
