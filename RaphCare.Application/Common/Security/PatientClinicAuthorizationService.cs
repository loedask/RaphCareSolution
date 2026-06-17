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
    IPatientClinicAccessService patientClinicAccessService)
    : IPatientClinicAuthorizationService
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IRepository<Patient> _patientRepository = patientRepository;
    private readonly IPatientClinicAccessService _patientClinicAccessService = patientClinicAccessService;

    public async Task<Patient> GetCurrentPatientAsync(CancellationToken cancellationToken)
    {
        var patientId = _currentUserService.CurrentPatientId;
        if (patientId is not null)
        {
            var patientFromToken = await _patientRepository.GetByIdAsync(patientId.Value, cancellationToken);
            return patientFromToken ?? throw new ForbiddenAccessException("No patient record linked to this token.");
        }

        // Backward-compatible fallback: locate patient by ApplicationUserId.
        var userId = _currentUserService.CurrentUserId
            ?? throw new ForbiddenAccessException("Unauthenticated user.");

        var patients = await _patientRepository.ListAsync(cancellationToken);
        var patient = patients.FirstOrDefault(p => p.ApplicationUserId == userId);

        return patient ?? throw new ForbiddenAccessException("No patient record linked to this user.");
    }

    public async Task EnsurePatientClinicAccessAsync(Guid clinicId, CancellationToken cancellationToken)
    {
        var patient = await GetCurrentPatientAsync(cancellationToken);
        await _patientClinicAccessService.EnsureClinicAccessAsync(patient.Id, clinicId, cancellationToken);
    }
}

