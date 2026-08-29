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
                CalledAt = p.CalledAt,
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
                RequestedAt = l.RequestedAt,
                CalledAt = l.CalledAt
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

    public async Task<CollectionDisplayBoardDto?> GetDisplayBoardAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var trimmed = token?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
            return null;

        var clinic = await clinicalDbContext.Clinics
            .AsNoTracking()
            .Where(c => c.CollectionDisplayToken == trimmed && c.IsActive)
            .Select(c => new { c.Id, c.Name })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        if (clinic is null)
            return null;

        var prescriptions = await clinicalDbContext.Set<Prescription>()
            .AsNoTracking()
            .Where(p => p.Visit.ClinicId == clinic.Id && p.Status == "Pending")
            .Select(p => new { p.PickupCode, Kind = CollectionDisplayBoardDto.PrescriptionKind, QueuedAt = p.IssuedAt, p.CalledAt })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var labOrders = await clinicalDbContext.Set<LabRequest>()
            .AsNoTracking()
            .Where(l => l.Visit.ClinicId == clinic.Id && l.Status == "Pending")
            .Select(l => new { l.PickupCode, Kind = CollectionDisplayBoardDto.LabKind, QueuedAt = l.RequestedAt, l.CalledAt })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var pending = prescriptions
            .Concat(labOrders)
            .Select(r => new DisplayRow(r.PickupCode, r.Kind, r.QueuedAt, r.CalledAt))
            .ToList();
        var nowServing = pending
            .Where(r => r.CalledAt is not null)
            .OrderByDescending(r => r.CalledAt)
            .Select(ToTicket)
            .FirstOrDefault();

        var waiting = pending
            .Where(r => nowServing is null || r.PickupCode != nowServing.PickupCode)
            .OrderBy(r => r.QueuedAt)
            .Take(8)
            .Select(ToTicket)
            .ToList();

        return new CollectionDisplayBoardDto
        {
            ClinicName = clinic.Name,
            NowServing = nowServing,
            Waiting = waiting
        };
    }

    private sealed record DisplayRow(string PickupCode, string Kind, DateTime QueuedAt, DateTime? CalledAt);

    private static CollectionDisplayTicketDto ToTicket(DisplayRow row) =>
        new()
        {
            PickupCode = row.PickupCode,
            Kind = row.Kind
        };
}
