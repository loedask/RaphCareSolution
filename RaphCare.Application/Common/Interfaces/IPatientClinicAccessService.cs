using RaphCare.Domain.Patients;

namespace RaphCare.Application.Common.Interfaces;

/// <summary>
/// Enforces patient->clinic access for multi-tenant safety.
/// </summary>
public interface IPatientClinicAccessService
{
    Task<bool> HasClinicAccessAsync(Guid patientId, Guid clinicId, CancellationToken ct);

    /// <summary>
    /// Ensures the patient has access to the clinic for the current request.
    /// Implementations may derive access from encounters and/or activate an existing access record.
    /// </summary>
    /// <exception cref="RaphCare.Application.Common.Exceptions.ForbiddenAccessException">
    /// Thrown when the patient does not have access to the clinic.
    /// </exception>
    Task EnsureClinicAccessAsync(Guid patientId, Guid clinicId, CancellationToken ct);

    /// <summary>
    /// Grants encounter-based clinic access by upserting a {@link PatientClinicAccess} row.
    /// </summary>
    Task GrantEncounterAccessAsync(Guid patientId, Guid clinicId, CancellationToken ct);

    /// <summary>
    /// Returns active clinic ids a patient can access.
    /// </summary>
    Task<Guid[]> GetAccessibleClinicIdsAsync(Guid patientId, CancellationToken ct);

    /// <summary>
    /// Grants or reactivates explicit manual clinic access for a patient.
    /// </summary>
    Task GrantManualAccessAsync(Guid patientId, Guid clinicId, string? notes, CancellationToken ct);
}

