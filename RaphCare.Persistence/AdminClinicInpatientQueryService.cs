using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Common;

namespace RaphCare.Persistence;

public sealed class AdminClinicInpatientQueryService(
    ClinicalDbContext clinicalDbContext,
    IDateTimeProvider clock)
    : IAdminClinicInpatientQueryService
{
    public async Task<AdminClinicInpatientBoardDto?> GetBoardAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default)
    {
        var clinic = await clinicalDbContext.Clinics
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == clinicId && !c.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (clinic is null)
            return null;

        var wards = await clinicalDbContext.Wards
            .AsNoTracking()
            .Where(w => w.ClinicId == clinicId)
            .Include(w => w.Facility)
            .Include(w => w.Rooms)
                .ThenInclude(r => r.Beds)
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var activeAdmissions = await clinicalDbContext.InpatientAdmissions
            .AsNoTracking()
            .Where(a => a.ClinicId == clinicId && a.Status == "Admitted")
            .Include(a => a.Patient)
            .Include(a => a.Bed)
                .ThenInclude(b => b.Room)
                    .ThenInclude(r => r.Ward)
                        .ThenInclude(w => w.Facility)
            .OrderByDescending(a => a.AdmittedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var admissionByBed = activeAdmissions
            .GroupBy(a => a.BedId)
            .ToDictionary(g => g.Key, g => g.First());

        var allBeds = wards.SelectMany(w => w.Rooms).SelectMany(r => r.Beds).Where(b => b.IsActive).ToList();
        var available = allBeds.Count(b => b.Status == "Available");
        var occupied = allBeds.Count(b => b.Status == "Occupied");
        var maintenance = allBeds.Count(b => b.Status == "Maintenance");
        var (todayStartUtc, todayEndUtc) = ClinicTimeZoneHelper.GetClinicDayUtcRange(clock.UtcNow, clinic.TimeZone);
        var stayStats = await LoadStayStatsAsync(clinicId, todayStartUtc, todayEndUtc, cancellationToken)
            .ConfigureAwait(false);

        return new AdminClinicInpatientBoardDto
        {
            ClinicId = clinicId,
            TotalBeds = allBeds.Count,
            AvailableBeds = available,
            OccupiedBeds = occupied,
            MaintenanceBeds = maintenance,
            ActiveAdmissions = activeAdmissions.Count,
            OccupancyPercent = allBeds.Count == 0 ? 0 : (int)Math.Round(100d * occupied / allBeds.Count),
            AdmissionsTodayCount = stayStats.AdmissionsToday,
            DischargesTodayCount = stayStats.DischargesToday,
            AverageLengthOfStayDays = stayStats.AverageStayDays,
            Wards = wards.Select(w => new AdminClinicWardDto
            {
                Id = w.Id,
                FacilityId = w.FacilityId,
                FacilityName = w.Facility?.Name ?? "Facility",
                Name = w.Name,
                Code = w.Code,
                IsActive = w.IsActive,
                Rooms = w.Rooms
                    .OrderBy(r => r.Name)
                    .Select(r => new AdminClinicRoomDto
                    {
                        Id = r.Id,
                        WardId = r.WardId,
                        Name = r.Name,
                        RoomType = r.RoomType,
                        IsActive = r.IsActive,
                        Beds = r.Beds
                            .OrderBy(b => b.Label)
                            .Select(b =>
                            {
                                admissionByBed.TryGetValue(b.Id, out var adm);
                                return new AdminClinicBedDto
                                {
                                    Id = b.Id,
                                    RoomId = b.RoomId,
                                    Label = b.Label,
                                    Status = b.Status,
                                    IsActive = b.IsActive,
                                    CurrentAdmissionId = adm?.Id,
                                    CurrentPatientId = adm?.PatientId,
                                    CurrentPatientName = adm?.Patient is null
                                        ? null
                                        : $"{adm.Patient.FirstName} {adm.Patient.LastName}".Trim()
                                };
                            }).ToList()
                    }).ToList()
            }).ToList(),
            ActiveAdmissionsList = activeAdmissions.Select(MapAdmission).ToList()
        };
    }

    public async Task<PagedResult<AdminClinicAdmissionDto>?> GetAdmissionsAsync(
        Guid clinicId,
        int pageNumber,
        int pageSize,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var clinicExists = await clinicalDbContext.Clinics
            .AsNoTracking()
            .AnyAsync(c => c.Id == clinicId && !c.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (!clinicExists)
            return null;

        var query = clinicalDbContext.InpatientAdmissions
            .AsNoTracking()
            .Where(a => a.ClinicId == clinicId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalized = status.Trim();
            query = query.Where(a => a.Status == normalized);
        }

        var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await query
            .Include(a => a.Patient)
            .Include(a => a.Bed)
                .ThenInclude(b => b.Room)
                    .ThenInclude(r => r.Ward)
                        .ThenInclude(w => w.Facility)
            .OrderByDescending(a => a.AdmittedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<AdminClinicAdmissionDto>
        {
            Items = items.Select(MapAdmission).ToList(),
            TotalCount = total,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<AdminClinicAdmissionDto?> GetAdmissionByIdAsync(
        Guid clinicId,
        Guid admissionId,
        CancellationToken cancellationToken = default)
    {
        var admission = await clinicalDbContext.InpatientAdmissions
            .AsNoTracking()
            .Where(a => a.Id == admissionId && a.ClinicId == clinicId)
            .Include(a => a.Patient)
            .Include(a => a.Bed)
                .ThenInclude(b => b.Room)
                    .ThenInclude(r => r.Ward)
                        .ThenInclude(w => w.Facility)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return admission is null ? null : MapAdmission(admission);
    }

    private static AdminClinicAdmissionDto MapAdmission(Domain.Clinical.InpatientAdmission a) => new()
    {
        Id = a.Id,
        PatientId = a.PatientId,
        PatientName = a.Patient is null ? "Patient" : $"{a.Patient.FirstName} {a.Patient.LastName}".Trim(),
        BedId = a.BedId,
        BedLabel = a.Bed?.Label ?? "—",
        RoomName = a.Bed?.Room?.Name ?? "—",
        WardName = a.Bed?.Room?.Ward?.Name ?? "—",
        FacilityName = a.Bed?.Room?.Ward?.Facility?.Name ?? "—",
        AdmittedAt = a.AdmittedAt,
        DischargedAt = a.DischargedAt,
        Status = a.Status,
        Reason = a.Reason,
        Notes = a.Notes,
        DischargeSummary = a.DischargeSummary,
        InvoiceId = a.InvoiceId
    };

    private async Task<(int AdmissionsToday, int DischargesToday, decimal? AverageStayDays)> LoadStayStatsAsync(
        Guid clinicId,
        DateTime todayStartUtc,
        DateTime todayEndUtc,
        CancellationToken cancellationToken)
    {
        var admissionsToday = await clinicalDbContext.InpatientAdmissions
            .AsNoTracking()
            .CountAsync(
                a => a.ClinicId == clinicId
                     && a.AdmittedAt >= todayStartUtc
                     && a.AdmittedAt <= todayEndUtc,
                cancellationToken)
            .ConfigureAwait(false);

        var dischargesToday = await clinicalDbContext.InpatientAdmissions
            .AsNoTracking()
            .CountAsync(
                a => a.ClinicId == clinicId
                     && a.DischargedAt != null
                     && a.DischargedAt >= todayStartUtc
                     && a.DischargedAt <= todayEndUtc,
                cancellationToken)
            .ConfigureAwait(false);

        var cutoff = clock.UtcNow.AddDays(-90);
        var discharged = await clinicalDbContext.InpatientAdmissions
            .AsNoTracking()
            .Where(a => a.ClinicId == clinicId
                        && a.Status == "Discharged"
                        && a.DischargedAt != null
                        && a.DischargedAt >= cutoff)
            .Select(a => new { a.AdmittedAt, a.DischargedAt })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        decimal? averageStay = discharged.Count == 0
            ? null
            : Math.Round(
                (decimal)discharged.Average(a => (a.DischargedAt!.Value - a.AdmittedAt).TotalDays),
                1);

        return (admissionsToday, dischargesToday, averageStay);
    }
}
