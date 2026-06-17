using RaphCare.Domain.Patients;

namespace RaphCare.Application.Common.Interfaces;

/// <summary>
/// Master Patient Index (MPI) matching service. Finds an existing patient by national health ID,
/// external system ID, phone, or demographics before creating a new record.
/// </summary>
public interface IMasterPatientIndexService
{
    /// <summary>
    /// Finds a single matching patient using priority: NationalHealthId, External (SourceSystem+ExternalId),
    /// PhoneNumber, then demographic (FirstName, LastName, DateOfBirth). Returns null if no match or multiple matches (ambiguous).
    /// </summary>
    Task<Patient?> FindMatchAsync(
        string? nationalHealthId,
        string? sourceSystem,
        string? externalId,
        string firstName,
        string lastName,
        DateTime dateOfBirth,
        string? phoneNumber,
        CancellationToken ct);
}
