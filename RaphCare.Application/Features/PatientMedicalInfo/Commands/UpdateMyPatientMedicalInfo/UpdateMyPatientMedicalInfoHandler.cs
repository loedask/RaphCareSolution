using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.PatientMedicalInfo.Commands.UpdateMyPatientMedicalInfo;

public sealed class UpdateMyPatientMedicalInfoHandler(
    IRepository<Patient> patients,
    IRepository<Domain.Patients.PatientProfile> profiles,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser) : IRequestHandler<UpdateMyPatientMedicalInfoCommand, Unit>
{
    private readonly IRepository<Patient> _patients = patients;
    private readonly IRepository<Domain.Patients.PatientProfile> _profiles = profiles;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<Unit> Handle(UpdateMyPatientMedicalInfoCommand request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        _ = await _patients.GetByIdAsync(patientId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException(nameof(Patient), patientId);

        var page = await _profiles.SearchAsync(
            q => q.Where(p => p.PatientId == patientId),
            1,
            1,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var profile = page.Items.FirstOrDefault();
        if (profile is null)
        {
            profile = new Domain.Patients.PatientProfile { PatientId = patientId };
            await _profiles.AddAsync(profile, cancellationToken).ConfigureAwait(false);
        }

        profile.BloodType = string.IsNullOrWhiteSpace(request.BloodType) ? null : request.BloodType.Trim();
        profile.SelfReportedAllergies = string.IsNullOrWhiteSpace(request.Allergies) ? null : request.Allergies.Trim();
        profile.SelfReportedChronicConditions = string.IsNullOrWhiteSpace(request.ChronicConditions) ? null : request.ChronicConditions.Trim();
        profile.SelfReportedMedications = string.IsNullOrWhiteSpace(request.Medications) ? null : request.Medications.Trim();
        profile.PrimaryCareProviderName = string.IsNullOrWhiteSpace(request.PrimaryDoctor) ? null : request.PrimaryDoctor.Trim();

        if (page.Items.Count > 0)
            await _profiles.UpdateAsync(profile, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
