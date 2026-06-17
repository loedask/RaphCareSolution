namespace RaphCare.Application.Common.Interfaces;

/// <summary>
/// Service for merging duplicate patient records into a primary record during Master Patient Index reconciliation.
/// Reassigns all related entities to the primary patient and soft-deletes the duplicate.
/// </summary>
public interface IPatientMergeService
{
    /// <summary>
    /// Merges the duplicate patient into the primary patient. All references are moved to the primary,
    /// the duplicate is soft-deleted, and the merge is recorded in PatientMergeHistory.
    /// </summary>
    /// <param name="primaryPatientId">The patient record to keep.</param>
    /// <param name="duplicatePatientId">The duplicate patient record to merge and soft-delete.</param>
    /// <param name="ct">Cancellation token.</param>
    Task MergePatientsAsync(Guid primaryPatientId, Guid duplicatePatientId, CancellationToken ct);
}
