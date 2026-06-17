using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Patients;
using RaphCare.Domain.Patients.Enums;

namespace RaphCare.Persistence;

/// <summary>
/// Derives/activates patient->clinic access from encounters and an explicit access table.
/// </summary>
public class PatientClinicAccessService(
    ClinicalDbContext clinicalDbContext,
    IDateTimeProvider clock) : IPatientClinicAccessService
{
    private readonly ClinicalDbContext _clinicalDbContext = clinicalDbContext ?? throw new ArgumentNullException(nameof(clinicalDbContext));
    private readonly IDateTimeProvider _clock = clock ?? throw new ArgumentNullException(nameof(clock));

    public async Task<bool> HasClinicAccessAsync(Guid patientId, Guid clinicId, CancellationToken ct)
    {
        // 1. Explicit active grant (fast path)
        var explicitGrant = await _clinicalDbContext.PatientClinicAccesses
            .FirstOrDefaultAsync(a => a.PatientId == patientId && a.ClinicId == clinicId && a.IsActive, ct)
            .ConfigureAwait(false);

        if (explicitGrant != null)
            return true;

        // 2. Derive access from encounters
        var grantedByRule = await TryDeriveGrantedByRuleAsync(patientId, clinicId, ct).ConfigureAwait(false);
        if (grantedByRule is null)
            return false;

        await UpsertEncounterAccessAsync(patientId, clinicId, grantedByRule, ct).ConfigureAwait(false);
        return true;
    }

    public async Task EnsureClinicAccessAsync(Guid patientId, Guid clinicId, CancellationToken ct)
    {
        var hasAccess = await HasClinicAccessAsync(patientId, clinicId, ct).ConfigureAwait(false);
        if (!hasAccess)
            throw new ForbiddenAccessException("Patient does not have access to this clinic.");
    }

    public async Task GrantEncounterAccessAsync(Guid patientId, Guid clinicId, CancellationToken ct)
    {
        // Try to infer why the access exists from current encounter data; fallback to a generic rule.
        var grantedByRule = await TryDeriveGrantedByRuleAsync(patientId, clinicId, ct).ConfigureAwait(false)
            ?? "Encounter";

        await UpsertEncounterAccessAsync(patientId, clinicId, grantedByRule, ct).ConfigureAwait(false);
    }

    public async Task<Guid[]> GetAccessibleClinicIdsAsync(Guid patientId, CancellationToken ct)
    {
        var explicitClinicIds = await _clinicalDbContext.PatientClinicAccesses
            .Where(a => a.PatientId == patientId && a.IsActive)
            .Select(a => a.ClinicId)
            .Distinct()
            .ToListAsync(ct)
            .ConfigureAwait(false);

        // Supplement access based on encounters so we don't require the upsert to have happened yet.
        var visitClinicIds = await _clinicalDbContext.Visits
            .Where(v => v.PatientId == patientId)
            .Select(v => v.ClinicId)
            .Distinct()
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var appointmentClinicIds = await _clinicalDbContext.Appointments
            .Where(a => a.PatientId == patientId)
            .Select(a => a.ClinicId)
            .Distinct()
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var teleClinicIds = await _clinicalDbContext.TeleSessions
            .Where(t => t.PatientId == patientId)
            .Select(t => t.ClinicId)
            .Distinct()
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return explicitClinicIds
            .Concat(visitClinicIds)
            .Concat(appointmentClinicIds)
            .Concat(teleClinicIds)
            .Distinct()
            .ToArray();
    }

    private async Task<string?> TryDeriveGrantedByRuleAsync(Guid patientId, Guid clinicId, CancellationToken ct)
    {
        if (await _clinicalDbContext.Visits.AnyAsync(v => v.PatientId == patientId && v.ClinicId == clinicId, ct).ConfigureAwait(false))
            return "Visit";

        if (await _clinicalDbContext.Appointments.AnyAsync(a => a.PatientId == patientId && a.ClinicId == clinicId, ct).ConfigureAwait(false))
            return "Appointment";

        if (await _clinicalDbContext.TeleSessions.AnyAsync(t => t.PatientId == patientId && t.ClinicId == clinicId, ct).ConfigureAwait(false))
            return "TeleSession";

        return null;
    }

    private async Task UpsertEncounterAccessAsync(Guid patientId, Guid clinicId, string grantedByRule, CancellationToken ct)
    {
        var now = _clock.UtcNow;

        var existing = await _clinicalDbContext.PatientClinicAccesses
            .FirstOrDefaultAsync(a => a.PatientId == patientId && a.ClinicId == clinicId, ct)
            .ConfigureAwait(false);

        if (existing is null)
        {
            _clinicalDbContext.PatientClinicAccesses.Add(new PatientClinicAccess
            {
                PatientId = patientId,
                ClinicId = clinicId,
                AccessType = PatientClinicAccessType.EncounterBased,
                GrantedAt = now,
                GrantedByRule = grantedByRule,
                LastValidatedAt = now,
                IsActive = true
            });
        }
        else
        {
            existing.AccessType = PatientClinicAccessType.EncounterBased;
            existing.GrantedAt = now;
            existing.GrantedByRule = grantedByRule;
            existing.LastValidatedAt = now;
            existing.IsActive = true;
            existing.Notes ??= null;

            _clinicalDbContext.PatientClinicAccesses.Update(existing);
        }

        // Persist grant so future reads/authorizations are stable.
        await _clinicalDbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}

