using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Common;
using RaphCare.Domain.Organization;

namespace RaphCare.Persistence;

public sealed class AdminClinicConsultQueryService(
    ClinicalDbContext clinicalDbContext,
    IProfessionalUserLookupService professionalUserLookupService,
    IDateTimeProvider clock)
    : IAdminClinicConsultQueryService
{
    public async Task<AdminClinicConsultBoardDto?> GetBoardAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default)
    {
        var clinic = await clinicalDbContext.Clinics
            .AsNoTracking()
            .Where(c => c.Id == clinicId && !c.IsDeleted)
            .Select(c => new { c.Name, c.TimeZone })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        if (clinic is null)
            return null;

        var tickets = await clinicalDbContext.ConsultTickets
            .AsNoTracking()
            .Where(t => t.ClinicId == clinicId)
            .OrderByDescending(t => t.ArrivedAt)
            .Take(200)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var patientIds = tickets
            .Where(t => t.PatientId.HasValue)
            .Select(t => t.PatientId!.Value)
            .Distinct()
            .ToList();

        var patients = await clinicalDbContext.Patients
            .AsNoTracking()
            .Where(p => patientIds.Contains(p.Id))
            .Select(p => new { p.Id, p.FirstName, p.LastName })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var patientLookup = patients.ToDictionary(p => p.Id);

        var appointmentIds = tickets
            .Where(t => t.AppointmentId.HasValue)
            .Select(t => t.AppointmentId!.Value)
            .Distinct()
            .ToList();
        var appointments = await clinicalDbContext.Appointments
            .AsNoTracking()
            .Where(a => appointmentIds.Contains(a.Id))
            .Select(a => new { a.Id, a.ScheduledStart })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var appointmentLookup = appointments.ToDictionary(a => a.Id);

        var providerIds = tickets
            .Where(t => t.ProviderId.HasValue)
            .Select(t => t.ProviderId!.Value)
            .Distinct()
            .ToList();
        var providerNames = await ResolveProviderNamesAsync(providerIds, cancellationToken)
            .ConfigureAwait(false);

        AdminClinicConsultTicketDto Map(ConsultTicket ticket)
        {
            string? name = null;
            if (ticket.PatientId is Guid patientId && patientLookup.TryGetValue(patientId, out var patient))
                name = $"{patient.FirstName} {patient.LastName}".Trim();

            string? providerName = null;
            if (ticket.ProviderId is Guid providerId)
                providerNames.TryGetValue(providerId, out providerName);

            DateTime? scheduledStart = null;
            if (ticket.AppointmentId is Guid appointmentId
                && appointmentLookup.TryGetValue(appointmentId, out var appointment))
                scheduledStart = appointment.ScheduledStart;

            return new AdminClinicConsultTicketDto
            {
                Id = ticket.Id,
                AppointmentId = ticket.AppointmentId,
                PatientId = ticket.PatientId,
                PatientName = name,
                ProviderId = ticket.ProviderId,
                ProviderName = providerName,
                QueueCode = ticket.QueueCode,
                Status = ticket.Status,
                ArrivedAt = ticket.ArrivedAt,
                CalledAt = ticket.CalledAt,
                CompletedAt = ticket.CompletedAt,
                ScheduledStart = scheduledStart
            };
        }

        var waiting = tickets
            .Where(t => t.Status == "Waiting")
            .OrderBy(t => t.ArrivedAt)
            .Select(Map)
            .ToList();
        var called = tickets
            .Where(t => t.Status == "Called")
            .OrderByDescending(t => t.CalledAt)
            .Select(Map)
            .ToList();
        var recent = tickets
            .Where(t => t.Status is "Completed" or "Cancelled")
            .OrderByDescending(t => t.CompletedAt ?? t.UpdatedAt ?? t.ArrivedAt)
            .Take(20)
            .Select(Map)
            .ToList();

        var (todayStartUtc, todayEndUtc) = ClinicTimeZoneHelper.GetClinicDayUtcRange(clock.UtcNow, clinic.TimeZone);
        var queuedAppointmentIds = tickets
            .Where(t => t.AppointmentId.HasValue && t.Status is "Waiting" or "Called")
            .Select(t => t.AppointmentId!.Value)
            .ToHashSet();

        var todayRows = await clinicalDbContext.Appointments
            .AsNoTracking()
            .Where(a => a.ClinicId == clinicId
                        && !a.IsCancelled
                        && (a.Status == "Scheduled" || a.Status == "InProgress")
                        && a.ScheduledStart >= todayStartUtc
                        && a.ScheduledStart <= todayEndUtc)
            .OrderBy(a => a.ScheduledStart)
            .Select(a => new
            {
                a.Id,
                a.PatientId,
                a.ProviderId,
                a.ScheduledStart,
                a.Status,
                a.Reason
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var todayPatientIds = todayRows.Select(r => r.PatientId).Distinct().ToList();
        var todayPatients = await clinicalDbContext.Patients
            .AsNoTracking()
            .Where(p => todayPatientIds.Contains(p.Id))
            .Select(p => new { p.Id, p.FirstName, p.LastName })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var todayPatientLookup = todayPatients.ToDictionary(p => p.Id);

        var todayProviderIds = todayRows.Select(r => r.ProviderId).Distinct().ToList();
        var todayProviderNames = await ResolveProviderNamesAsync(todayProviderIds, cancellationToken)
            .ConfigureAwait(false);

        var todayAppointments = todayRows.Select(row =>
        {
            todayPatientLookup.TryGetValue(row.PatientId, out var patient);
            todayProviderNames.TryGetValue(row.ProviderId, out var providerName);
            return new AdminClinicConsultAppointmentDto
            {
                AppointmentId = row.Id,
                PatientId = row.PatientId,
                PatientName = patient is null
                    ? "Patient"
                    : $"{patient.FirstName} {patient.LastName}".Trim(),
                ProviderId = row.ProviderId,
                ProviderName = providerName ?? "Provider",
                ScheduledStart = row.ScheduledStart,
                Status = row.Status,
                Reason = row.Reason,
                AlreadyQueued = queuedAppointmentIds.Contains(row.Id)
            };
        }).ToList();

        return new AdminClinicConsultBoardDto
        {
            ClinicName = clinic.Name,
            WaitingCount = waiting.Count,
            CalledCount = called.Count,
            Waiting = waiting,
            Called = called,
            Recent = recent,
            TodayAppointments = todayAppointments
        };
    }

    public async Task<ConsultDisplayBoardDto?> GetDisplayBoardAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var trimmed = token?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
            return null;

        var clinic = await clinicalDbContext.Clinics
            .AsNoTracking()
            .Where(c => c.ConsultDisplayToken == trimmed && c.IsActive && !c.IsDeleted)
            .Select(c => new { c.Id, c.Name })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        if (clinic is null)
            return null;

        var open = await clinicalDbContext.ConsultTickets
            .AsNoTracking()
            .Where(t => t.ClinicId == clinic.Id && (t.Status == "Waiting" || t.Status == "Called"))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var nowServing = open
            .Where(t => t.Status == "Called" && t.CalledAt.HasValue)
            .OrderByDescending(t => t.CalledAt)
            .Select(t => new ConsultDisplayTicketDto { QueueCode = t.QueueCode })
            .FirstOrDefault();

        var waiting = open
            .Where(t => t.Status == "Waiting")
            .OrderBy(t => t.ArrivedAt)
            .Take(8)
            .Select(t => new ConsultDisplayTicketDto { QueueCode = t.QueueCode })
            .ToList();

        return new ConsultDisplayBoardDto
        {
            ClinicName = clinic.Name,
            NowServing = nowServing,
            Waiting = waiting
        };
    }

    private async Task<Dictionary<Guid, string>> ResolveProviderNamesAsync(
        List<Guid> providerIds,
        CancellationToken cancellationToken)
    {
        if (providerIds.Count == 0)
            return new Dictionary<Guid, string>();

        var providers = await clinicalDbContext.Set<Provider>()
            .AsNoTracking()
            .Where(p => providerIds.Contains(p.Id))
            .Select(p => new { p.Id, p.ApplicationUserId })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var userIds = providers.Select(p => p.ApplicationUserId).Distinct().ToList();
        var users = await professionalUserLookupService
            .GetUsersByIdsAsync(userIds, cancellationToken)
            .ConfigureAwait(false);
        var usersById = users.ToDictionary(u => u.Id);
        return providers.ToDictionary(
            p => p.Id,
            p => usersById.TryGetValue(p.ApplicationUserId, out var user)
                ? (string.IsNullOrWhiteSpace(user.DisplayName) ? user.Email : user.DisplayName)
                : "Provider");
    }
}
