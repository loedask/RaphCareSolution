using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Patients;

namespace RaphCare.Persistence;

/// <summary>
/// Resolves existing patient id by NationalHealthId or (SourceSystem, ExternalId) for unique constraint conflict recovery.
/// </summary>
public class PatientUniqueConflictResolver(ClinicalDbContext context) : IPatientUniqueConflictResolver
{
    private readonly ClinicalDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <inheritdoc />
    public async Task<Guid?> ResolveExistingPatientIdAsync(
        string? nationalHealthId,
        string? sourceSystem,
        string? externalId,
        CancellationToken cancellationToken = default)
    {
        // 1. Try by NationalHealthId
        if (!string.IsNullOrWhiteSpace(nationalHealthId))
        {
            var nid = nationalHealthId.Trim();
            var existing = await _context.Patients
                .AsNoTracking()
                .Where(p => p.NationalHealthId == nid)
                .Select(p => p.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (existing != Guid.Empty)
                return existing;
        }

        // 2. Try by SourceSystem + ExternalId
        if (!string.IsNullOrWhiteSpace(sourceSystem) && !string.IsNullOrWhiteSpace(externalId))
        {
            var mapping = await _context.PatientExternalIds
                .AsNoTracking()
                .Where(e => e.SourceSystem == sourceSystem.Trim() && e.ExternalId == externalId.Trim())
                .Select(e => e.PatientId)
                .FirstOrDefaultAsync(cancellationToken);
            if (mapping != Guid.Empty)
                return mapping;
        }

        return null;
    }
}
