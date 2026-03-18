using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Events;
using RaphCare.Domain.Patients;
using RaphCare.Domain.Patients.Enums;

namespace RaphCare.Persistence;

/// <summary>
/// Merges duplicate patient records into a primary record for MPI reconciliation.
/// Reassigns all related entities to the primary, respects PatientExternalId uniqueness, soft-deletes the duplicate, and records the merge in PatientMergeHistory.
/// </summary>
public class PatientMergeService(
    ClinicalDbContext clinical,
    InsuranceDbContext insurance,
    DeviceDbContext device,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IMediator mediator,
    IPatientIdentityTimelineService patientIdentityTimelineService) : IPatientMergeService
{
    private readonly ClinicalDbContext _clinical = clinical ?? throw new ArgumentNullException(nameof(clinical));
    private readonly InsuranceDbContext _insurance = insurance ?? throw new ArgumentNullException(nameof(insurance));
    private readonly DeviceDbContext _device = device ?? throw new ArgumentNullException(nameof(device));
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly ICurrentUserService _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    private readonly IPatientIdentityTimelineService _patientIdentityTimelineService = patientIdentityTimelineService ?? throw new ArgumentNullException(nameof(patientIdentityTimelineService));

    /// <inheritdoc />
    public async Task MergePatientsAsync(Guid primaryPatientId, Guid duplicatePatientId, CancellationToken ct)
    {
        // 1. Validate primary exists (query filter excludes soft-deleted)
        var primary = await _clinical.Patients.FindAsync([primaryPatientId], ct).ConfigureAwait(false);
        if (primary == null)
            throw new InvalidOperationException($"Primary patient not found: {primaryPatientId}.");

        // Load duplicate with filter ignored so we can detect already-merged (soft-deleted) and enforce idempotency
        var duplicate = await _clinical.Patients
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == duplicatePatientId, ct).ConfigureAwait(false);
        if (duplicate == null)
            throw new InvalidOperationException($"Duplicate patient not found: {duplicatePatientId}.");
        if (duplicate.IsDeleted)
            return; // Idempotency: already merged, no-op to avoid duplicate merge or duplicate event

        // 2. Prevent merging a patient with itself
        if (primaryPatientId == duplicatePatientId)
            throw new InvalidOperationException("Cannot merge a patient with itself.");

        // 3. Move all related entities from duplicate to primary (single logical transaction across contexts)

        // Clinical DB: Visits, Appointments, TeleSessions, VoiceRecordings, CarePlans
        await _clinical.Visits
            .Where(v => v.PatientId == duplicatePatientId)
            .ExecuteUpdateAsync(s => s.SetProperty(v => v.PatientId, primaryPatientId), ct).ConfigureAwait(false);
        await _clinical.Appointments
            .Where(a => a.PatientId == duplicatePatientId)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.PatientId, primaryPatientId), ct).ConfigureAwait(false);
        await _clinical.TeleSessions
            .Where(t => t.PatientId == duplicatePatientId)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.PatientId, primaryPatientId), ct).ConfigureAwait(false);
        await _clinical.VoiceRecordings
            .Where(r => r.PatientId == duplicatePatientId)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.PatientId, primaryPatientId), ct).ConfigureAwait(false);
        await _clinical.CarePlans
            .Where(c => c.PatientId == duplicatePatientId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.PatientId, primaryPatientId), ct).ConfigureAwait(false);

        // Clinical DB: entities exposed via Set<> (same database as per InitialClinical migration)
        await _clinical.Set<MedicalHistory>()
            .Where(m => m.PatientId == duplicatePatientId)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.PatientId, primaryPatientId), ct).ConfigureAwait(false);
        await _clinical.Set<Medication>()
            .Where(m => m.PatientId == duplicatePatientId)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.PatientId, primaryPatientId), ct).ConfigureAwait(false);
        await _clinical.Set<Allergy>()
            .Where(a => a.PatientId == duplicatePatientId)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.PatientId, primaryPatientId), ct).ConfigureAwait(false);
        await _clinical.Set<ChronicCondition>()
            .Where(c => c.PatientId == duplicatePatientId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.PatientId, primaryPatientId), ct).ConfigureAwait(false);

        // 4. PatientExternalIds: ensure uniqueness (SourceSystem, ExternalId). Remove conflicting rows, then reassign the rest.
        var primaryPairs = await _clinical.PatientExternalIds
            .Where(e => e.PatientId == primaryPatientId)
            .Select(e => new { e.SourceSystem, e.ExternalId })
            .ToListAsync(ct).ConfigureAwait(false);
        var primarySet = primaryPairs.Select(k => (k.SourceSystem, k.ExternalId)).ToHashSet();
        var duplicateExternals = await _clinical.PatientExternalIds
            .Where(e => e.PatientId == duplicatePatientId)
            .Select(e => new { e.Id, e.SourceSystem, e.ExternalId })
            .ToListAsync(ct).ConfigureAwait(false);
        var conflictIds = duplicateExternals
            .Where(d => primarySet.Contains((d.SourceSystem, d.ExternalId)))
            .Select(d => d.Id)
            .ToList();
        if (conflictIds.Count > 0)
            await _clinical.PatientExternalIds
                .Where(e => conflictIds.Contains(e.Id))
                .ExecuteDeleteAsync(ct).ConfigureAwait(false);
        await _clinical.PatientExternalIds
            .Where(e => e.PatientId == duplicatePatientId)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.PatientId, primaryPatientId), ct).ConfigureAwait(false);

        // Insurance DB
        await _insurance.InsuranceProfiles
            .Where(i => i.PatientId == duplicatePatientId)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.PatientId, primaryPatientId), ct).ConfigureAwait(false);

        // Device DB: assignments linked to patient
        await _device.DeviceAssignments
            .Where(d => d.PatientId == duplicatePatientId)
            .ExecuteUpdateAsync(s => s.SetProperty(d => d.PatientId, primaryPatientId), ct).ConfigureAwait(false);

        // 5. Soft delete the duplicate patient
        duplicate.IsDeleted = true;
        duplicate.DeletedAt = DateTime.UtcNow;
        _clinical.Patients.Update(duplicate);

        // 6. Merge audit logging
        var mergedAt = DateTime.UtcNow;
        var mergedBy = _currentUserService.CurrentUserId;
        _clinical.PatientMergeHistory.Add(new PatientMergeHistory
        {
            PrimaryPatientId = primaryPatientId,
            MergedPatientId = duplicatePatientId,
            MergedAt = mergedAt,
            MergedByUserId = mergedBy
        });

        await _patientIdentityTimelineService.RecordEventAsync(
            primaryPatientId,
            PatientIdentityEventType.PatientMerged,
            new
            {
                primaryPatientId,
                mergedPatientId = duplicatePatientId,
                mergedAt,
                mergedByUserId = mergedBy
            },
            performedByUserId: mergedBy,
            ct);

        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        // 7. Publish domain event for handlers (e.g. Application Insights logging, cache invalidation, MPI reconciliation, external integrations)
        await _mediator.Publish(new PatientMergedEvent(primaryPatientId, duplicatePatientId, mergedAt, mergedBy), ct).ConfigureAwait(false);
    }
}
