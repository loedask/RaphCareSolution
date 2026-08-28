using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Persistence;

public sealed class AdminClinicAppointmentQueryService(
    ClinicalDbContext clinicalDbContext,
    IProfessionalUserLookupService professionalUserLookupService)
    : IAdminClinicAppointmentQueryService
{
    public async Task<PagedResult<AdminClinicAppointmentListItemDto>> GetAppointmentsAsync(
        Guid clinicId,
        int pageNumber,
        int pageSize,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = clinicalDbContext.Appointments
            .AsNoTracking()
            .Where(a => a.ClinicId == clinicId && !a.IsCancelled);

        if (fromUtc is { } from)
            query = query.Where(a => a.ScheduledStart >= from);

        if (toUtc is { } to)
            query = query.Where(a => a.ScheduledStart <= to);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalized = status.Trim();
            query = query.Where(a => a.Status == normalized);
        }

        query = query.OrderBy(a => a.ScheduledStart);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var skip = (pageNumber - 1) * pageSize;

        var rows = await query
            .Skip(skip)
            .Take(pageSize)
            .Select(a => new
            {
                a.Id,
                a.PatientId,
                a.ProviderId,
                a.ScheduledStart,
                a.ScheduledEnd,
                a.Type,
                a.Status,
                a.Reason
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (rows.Count == 0)
        {
            return new PagedResult<AdminClinicAppointmentListItemDto>
            {
                Items = [],
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        var patientIds = rows.Select(r => r.PatientId).Distinct().ToList();
        var patients = await clinicalDbContext.Patients
            .AsNoTracking()
            .Where(p => patientIds.Contains(p.Id))
            .Select(p => new { p.Id, p.FirstName, p.LastName })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var patientsById = patients.ToDictionary(p => p.Id);

        var providerIds = rows.Select(r => r.ProviderId).Distinct().ToList();
        var providers = await clinicalDbContext.Set<Domain.Organization.Provider>()
            .AsNoTracking()
            .Where(p => providerIds.Contains(p.Id))
            .Select(p => new { p.Id, p.ApplicationUserId })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var providerUserIds = providers.Select(p => p.ApplicationUserId).Distinct().ToList();
        var users = await professionalUserLookupService
            .GetUsersByIdsAsync(providerUserIds, cancellationToken)
            .ConfigureAwait(false);
        var usersById = users.ToDictionary(u => u.Id);
        var providerNames = providers.ToDictionary(
            p => p.Id,
            p => usersById.TryGetValue(p.ApplicationUserId, out var u)
                ? (string.IsNullOrWhiteSpace(u.DisplayName) ? u.Email : u.DisplayName)
                : "Provider");

        var appointmentIds = rows.Select(r => r.Id).ToList();
        var openVisits = await clinicalDbContext.Visits
            .AsNoTracking()
            .Where(v => appointmentIds.Contains(v.AppointmentId)
                        && v.Status != "Completed"
                        && v.Status != "Cancelled")
            .Select(v => new { v.AppointmentId, v.Id })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var visitByAppointment = openVisits
            .GroupBy(v => v.AppointmentId)
            .ToDictionary(g => g.Key, g => g.First().Id);

        var items = rows.Select(r =>
        {
            patientsById.TryGetValue(r.PatientId, out var patient);
            providerNames.TryGetValue(r.ProviderId, out var providerName);
            visitByAppointment.TryGetValue(r.Id, out var visitId);
            return new AdminClinicAppointmentListItemDto
            {
                Id = r.Id,
                PatientId = r.PatientId,
                PatientName = patient is null ? "Patient" : $"{patient.FirstName} {patient.LastName}".Trim(),
                ProviderId = r.ProviderId,
                ProviderName = providerName ?? "Provider",
                ScheduledStart = r.ScheduledStart,
                ScheduledEnd = r.ScheduledEnd,
                Type = r.Type,
                Status = r.Status,
                Reason = r.Reason,
                ActiveVisitId = visitId == Guid.Empty ? null : visitId
            };
        }).ToList();

        return new PagedResult<AdminClinicAppointmentListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
