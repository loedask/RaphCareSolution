namespace RaphCare.Application.Common.Interfaces;

/// <summary>
/// Resolves an existing patient id when a unique constraint violation occurs (e.g. duplicate NationalHealthId or (SourceSystem, ExternalId)).
/// Used to gracefully handle concurrent patient creation by returning the existing record.
/// </summary>
public interface IPatientUniqueConflictResolver
{
    /// <summary>
    /// Tries to find an existing patient by the same unique keys that may have caused the conflict.
    /// Tries NationalHealthId first, then (SourceSystem, ExternalId) if provided.
    /// </summary>
    /// <param name="nationalHealthId">National health id that was used on the failed insert (if any).</param>
    /// <param name="sourceSystem">Source system that was used for PatientExternalId (if any).</param>
    /// <param name="externalId">External id that was used for PatientExternalId (if any).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The existing patient's id if found; otherwise null.</returns>
    Task<Guid?> ResolveExistingPatientIdAsync(
        string? nationalHealthId,
        string? sourceSystem,
        string? externalId,
        CancellationToken cancellationToken = default);
}
