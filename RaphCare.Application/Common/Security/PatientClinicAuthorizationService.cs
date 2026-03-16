using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Common.Security;

/// <summary>
/// Helper for resolving the current patient from the authenticated user and enforcing clinic-scoped access.
/// </summary>
public class PatientClinicAuthorizationService(
    ICurrentUserService currentUserService,
    IRepository<Patient> patientRepository,
    IRepository<Visit> visitRepository)
    : IPatientClinicAuthorizationService
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IRepository<Patient> _patientRepository = patientRepository;
    private readonly IRepository<Visit> _visitRepository = visitRepository;

    public async Task<Patient> GetCurrentPatientAsync(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.CurrentUserId
            ?? throw new ForbiddenAccessException("Unauthenticated user.");

        var patients = await _patientRepository.ListAsync(cancellationToken);
        var patient = patients.FirstOrDefault(p => p.ApplicationUserId == userId);

        return patient
            ?? throw new ForbiddenAccessException("No patient record linked to this user.");
    }

    public async Task EnsurePatientClinicAccessAsync(Guid clinicId, CancellationToken cancellationToken)
    {
        var patient = await GetCurrentPatientAsync(cancellationToken);

        var visits = await _visitRepository.ListAsync(cancellationToken);
        var hasAccess = visits.Any(v => v.PatientId == patient.Id && v.ClinicId == clinicId);

        if (!hasAccess)
        {
            throw new ForbiddenAccessException("Patient does not belong to this clinic.");
        }
    }
}

