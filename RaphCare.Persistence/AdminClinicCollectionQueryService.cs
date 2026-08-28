using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Persistence;

public sealed class AdminClinicCollectionQueryService(ClinicalDbContext clinicalDbContext)
    : IAdminClinicCollectionQueryService
{
    public async Task<AdminClinicCollectionBoardDto?> GetPendingOrdersAsync(
        Guid clinicId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var clinicExists = await clinicalDbContext.Clinics
            .AsNoTracking()
            .AnyAsync(c => c.Id == clinicId && !c.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (!clinicExists)
            return null;

        var term = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

        var prescriptions = await clinicalDbContext.Set<Prescription>()
            .AsNoTracking()
            .Where(p => p.Visit.ClinicId == clinicId)
            .Include(p => p.PrescriptionItems)
            .Include(p => p.Visit)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var labOrders = await clinicalDbContext.Set<LabRequest>()
            .AsNoTracking()
            .Where(l => l.Visit.ClinicId == clinicId)
            .Include(l => l.Visit)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var patientIds = prescriptions.Select(p => p.Visit.PatientId)
            .Concat(labOrders.Select(l => l.Visit.PatientId))
            .Distinct()
            .ToList();

        var patients = await clinicalDbContext.Patients
            .AsNoTracking()
            .Where(p => patientIds.Contains(p.Id))
            .Select(p => new
            {
                p.Id,
                p.FirstName,
                p.LastName,
                p.NationalHealthId
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var patientLookup = patients.ToDictionary(p => p.Id);

        string Name(Guid patientId)
        {
            if (!patientLookup.TryGetValue(patientId, out var patient))
                return "Patient";
            return $"{patient.FirstName} {patient.LastName}".Trim();
        }

        bool Matches(Guid patientId, string pickupCode)
        {
            if (term is null)
                return true;
            if (pickupCode.Contains(term, StringComparison.OrdinalIgnoreCase))
                return true;
            if (!patientLookup.TryGetValue(patientId, out var patient))
                return false;
            var name = $"{patient.FirstName} {patient.LastName}";
            return name.Contains(term, StringComparison.OrdinalIgnoreCase)
                || (patient.NationalHealthId is not null
                    && patient.NationalHealthId.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        AdminClinicCollectionPrescriptionDto MapRx(Prescription p)
        {
            patientLookup.TryGetValue(p.Visit.PatientId, out var patient);
            return new AdminClinicCollectionPrescriptionDto
            {
                Id = p.Id,
                VisitId = p.VisitId,
                PatientId = p.Visit.PatientId,
                PatientName = Name(p.Visit.PatientId),
                NationalHealthId = patient?.NationalHealthId,
                PickupCode = p.PickupCode,
                IssuedAt = p.IssuedAt,
                Status = p.Status,
                DispensedAt = p.DispensedAt,
                Notes = p.Notes,
                Items = p.PrescriptionItems.Select(i => new AdminClinicVisitPrescriptionItemDto
                {
                    MedicationName = i.MedicationName,
                    Dosage = i.Dosage,
                    Frequency = i.Frequency,
                    DurationDays = i.DurationDays
                }).ToList()
            };
        }

        AdminClinicCollectionLabOrderDto MapLab(LabRequest l)
        {
            patientLookup.TryGetValue(l.Visit.PatientId, out var patient);
            return new AdminClinicCollectionLabOrderDto
            {
                Id = l.Id,
                VisitId = l.VisitId,
                PatientId = l.Visit.PatientId,
                PatientName = Name(l.Visit.PatientId),
                NationalHealthId = patient?.NationalHealthId,
                PickupCode = l.PickupCode,
                TestName = l.TestName,
                Status = l.Status,
                RequestedAt = l.RequestedAt
            };
        }

        var pendingRx = prescriptions
            .Where(p => p.Status == "Pending" && Matches(p.Visit.PatientId, p.PickupCode))
            .OrderBy(p => p.IssuedAt)
            .Select(MapRx)
            .ToList();
        var recentRx = prescriptions
            .Where(p => p.Status is "Dispensed" or "Cancelled" && Matches(p.Visit.PatientId, p.PickupCode))
            .OrderByDescending(p => p.DispensedAt ?? p.IssuedAt)
            .Take(50)
            .Select(MapRx)
            .ToList();
        var pendingLabs = labOrders
            .Where(l => l.Status == "Pending" && Matches(l.Visit.PatientId, l.PickupCode))
            .OrderBy(l => l.RequestedAt)
            .Select(MapLab)
            .ToList();
        var recentLabs = labOrders
            .Where(l => l.Status is "Completed" or "Cancelled" && Matches(l.Visit.PatientId, l.PickupCode))
            .OrderByDescending(l => l.RequestedAt)
            .Take(50)
            .Select(MapLab)
            .ToList();

        return new AdminClinicCollectionBoardDto
        {
            Prescriptions = pendingRx,
            LabOrders = pendingLabs,
            RecentPrescriptions = recentRx,
            RecentLabOrders = recentLabs
        };
    }
}
