using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Patients;

namespace RaphCare.Persistence;

/// <summary>
/// MPI matching using ClinicalDbContext. Applies priority: NationalHealthId, External ID, Phone, Demographics.
/// Returns null when no match or multiple matches (ambiguous).
/// </summary>
public class MasterPatientIndexService(ClinicalDbContext context) : IMasterPatientIndexService
{
    private readonly ClinicalDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<Patient?> FindMatchAsync(
        string? nationalHealthId,
        string? sourceSystem,
        string? externalId,
        string firstName,
        string lastName,
        DateTime dateOfBirth,
        string? phoneNumber,
        CancellationToken ct)
    {
        var fName = (firstName ?? string.Empty).Trim();
        var lName = (lastName ?? string.Empty).Trim();
        var dobDate = dateOfBirth.Date;

        // 1. NationalHealthId
        if (!string.IsNullOrWhiteSpace(nationalHealthId))
        {
            var nid = nationalHealthId.Trim();
            var byNid = await _context.Patients
                .Where(p => p.NationalHealthId == nid)
                .ToListAsync(ct);
            if (byNid.Count == 1) return byNid[0];
            if (byNid.Count > 1) return null; // ambiguous
        }

        // 2. External System ID (SourceSystem + ExternalId)
        if (!string.IsNullOrWhiteSpace(sourceSystem) && !string.IsNullOrWhiteSpace(externalId))
        {
            var src = sourceSystem.Trim();
            var ext = externalId.Trim();
            var patientIds = await _context.PatientExternalIds
                .Where(e => e.SourceSystem == src && e.ExternalId == ext)
                .Select(e => e.PatientId)
                .Distinct()
                .ToListAsync(ct);
            if (patientIds.Count == 1)
            {
                var patient = await _context.Patients.FindAsync([patientIds[0]], ct);
                return patient;
            }
            if (patientIds.Count > 1) return null; // ambiguous
        }

        // 3. Phone Number
        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            var phone = phoneNumber.Trim();
            var byPhone = await _context.Patients
                .Where(p => p.PhoneNumber == phone)
                .ToListAsync(ct);
            if (byPhone.Count == 1) return byPhone[0];
            if (byPhone.Count > 1) return null; // ambiguous
        }

        // 4. Demographic match (FirstName, LastName, DateOfBirth)
        var dobStart = dobDate;
        var dobEnd = dobDate.AddDays(1);
        var byDemo = await _context.Patients
            .Where(p =>
                p.FirstName == fName
                && p.LastName == lName
                && p.DateOfBirth >= dobStart
                && p.DateOfBirth < dobEnd)
            .ToListAsync(ct);
        if (byDemo.Count == 1) return byDemo[0];
        if (byDemo.Count > 1) return null; // ambiguous

        return null;
    }
}
