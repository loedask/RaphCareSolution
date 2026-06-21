using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Persistence;

public sealed class AdminClinicPatientQueryService(
    ClinicalDbContext clinicalDbContext,
    IProfessionalUserLookupService professionalUserLookupService)
    : IAdminClinicPatientQueryService
{
    public async Task<PagedResult<AdminClinicPatientListItemDto>> GetPatientsAsync(
        Guid clinicId,
        int pageNumber,
        int pageSize,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = clinicalDbContext.PatientClinicAccesses
            .AsNoTracking()
            .Where(a => a.ClinicId == clinicId && a.IsActive)
            .Join(
                clinicalDbContext.Patients.AsNoTracking().Where(p => !p.IsDeleted),
                access => access.PatientId,
                patient => patient.Id,
                (access, patient) => new { access, patient });

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.patient.FirstName.Contains(term)
                || x.patient.LastName.Contains(term)
                || (x.patient.Email != null && x.patient.Email.Contains(term))
                || (x.patient.PhoneNumber != null && x.patient.PhoneNumber.Contains(term)));
        }

        query = query
            .OrderBy(x => x.patient.LastName)
            .ThenBy(x => x.patient.FirstName);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var skip = (pageNumber - 1) * pageSize;

        var items = await query
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new AdminClinicPatientListItemDto
            {
                PatientId = x.patient.Id,
                FirstName = x.patient.FirstName,
                LastName = x.patient.LastName,
                DateOfBirth = x.patient.DateOfBirth,
                AccessType = x.access.AccessType.ToString(),
                GrantedAt = x.access.GrantedAt
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<AdminClinicPatientListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<AdminClinicPatientDetailDto?> GetPatientDetailAsync(
        Guid clinicId,
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        var row = await clinicalDbContext.PatientClinicAccesses
            .AsNoTracking()
            .Where(a => a.ClinicId == clinicId && a.PatientId == patientId && a.IsActive)
            .Join(
                clinicalDbContext.Patients.AsNoTracking().Where(p => !p.IsDeleted),
                access => access.PatientId,
                patient => patient.Id,
                (access, patient) => new { access, patient })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (row is null)
            return null;

        var recentVisits = await clinicalDbContext.Visits
            .AsNoTracking()
            .Where(v => v.ClinicId == clinicId && v.PatientId == patientId)
            .OrderByDescending(v => v.VisitStart)
            .Take(10)
            .Select(v => new AdminClinicPatientVisitDto
            {
                Id = v.Id,
                VisitStart = v.VisitStart,
                VisitEnd = v.VisitEnd,
                VisitType = v.VisitType,
                Status = v.Status,
                Summary = v.Summary
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var appointments = await clinicalDbContext.Appointments
            .AsNoTracking()
            .Where(a => a.ClinicId == clinicId && a.PatientId == patientId && !a.IsCancelled)
            .OrderByDescending(a => a.ScheduledStart)
            .Take(10)
            .Select(a => new
            {
                a.Id,
                a.ProviderId,
                a.ScheduledStart,
                a.ScheduledEnd,
                a.Type,
                a.Status,
                a.Reason
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var providerIds = appointments.Select(a => a.ProviderId).Distinct().ToList();
        var providers = providerIds.Count == 0
            ? []
            : await clinicalDbContext.Set<Domain.Organization.Provider>()
                .AsNoTracking()
                .Where(p => providerIds.Contains(p.Id))
                .Select(p => new { p.Id, p.ApplicationUserId })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

        var userIds = providers.Select(p => p.ApplicationUserId).Distinct().ToList();
        var users = userIds.Count == 0
            ? []
            : await professionalUserLookupService.GetUsersByIdsAsync(userIds, cancellationToken).ConfigureAwait(false);
        var usersById = users.ToDictionary(u => u.Id);
        var providerNames = providers.ToDictionary(
            p => p.Id,
            p => usersById.TryGetValue(p.ApplicationUserId, out var u)
                ? (string.IsNullOrWhiteSpace(u.DisplayName) ? u.Email : u.DisplayName) ?? "Provider"
                : "Provider");

        var appointmentDtos = appointments.Select(a =>
        {
            providerNames.TryGetValue(a.ProviderId, out var name);
            return new AdminClinicPatientAppointmentDto
            {
                Id = a.Id,
                ScheduledStart = a.ScheduledStart,
                ScheduledEnd = a.ScheduledEnd,
                Type = a.Type,
                Status = a.Status,
                Reason = a.Reason,
                ProviderName = name ?? "Provider"
            };
        }).ToList();

        return new AdminClinicPatientDetailDto
        {
            PatientId = row.patient.Id,
            FirstName = row.patient.FirstName,
            LastName = row.patient.LastName,
            DateOfBirth = row.patient.DateOfBirth,
            Gender = row.patient.Gender,
            Email = row.patient.Email,
            PhoneNumber = row.patient.PhoneNumber,
            AccessType = row.access.AccessType.ToString(),
            GrantedAt = row.access.GrantedAt,
            GrantedByRule = row.access.GrantedByRule,
            Notes = row.access.Notes,
            RecentVisits = recentVisits,
            Appointments = appointmentDtos
        };
    }
}
