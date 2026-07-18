using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Persistence;

public sealed class AdminClinicInpatientQueryService(ClinicalDbContext clinicalDbContext)
    : IAdminClinicInpatientQueryService
{
    public async Task<AdminClinicInpatientBoardDto?> GetBoardAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default)
    {
        var clinicExists = await clinicalDbContext.Clinics
            .AsNoTracking()
            .AnyAsync(c => c.Id == clinicId && !c.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (!clinicExists)
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

        return new AdminClinicInpatientBoardDto
        {
            ClinicId = clinicId,
            TotalBeds = allBeds.Count,
            AvailableBeds = available,
            OccupiedBeds = occupied,
            ActiveAdmissions = activeAdmissions.Count,
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
            ActiveAdmissionsList = activeAdmissions.Select(a => new AdminClinicAdmissionDto
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
                Notes = a.Notes
            }).ToList()
        };
    }
}
