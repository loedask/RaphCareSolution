using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Common.Security;

/// <summary>
/// Reusable guard for future patient-facing handlers.
/// Authorization comes from: patient identity (JWT) + clinic context + derived/explicit patient-clinic access.
/// Note: <c>X-Clinic-Id</c> provides request context only; it is not an authorization authority.
/// </summary>
public class PatientClinicAccessGuardService(
    ICurrentUserService currentUserService,
    IClinicContext clinicContext,
    IPatientClinicAccessService patientClinicAccessService)
{
    public async Task EnsureCurrentPatientCanAccessCurrentClinicAsync(CancellationToken ct)
    {
        var clinicId = clinicContext.ClinicId
            ?? throw new ForbiddenAccessException("Missing clinic context.");

        var patientId = currentUserService.CurrentPatientId
            ?? throw new ForbiddenAccessException("Missing patient identity in token.");

        await patientClinicAccessService.EnsureClinicAccessAsync(patientId, clinicId, ct).ConfigureAwait(false);
    }
}

