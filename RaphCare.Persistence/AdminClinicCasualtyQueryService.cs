using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Persistence;

public sealed class AdminClinicCasualtyQueryService(ClinicalDbContext clinicalDbContext)
    : IAdminClinicCasualtyQueryService
{
    public async Task<AdminClinicCasualtyBoardDto?> GetBoardAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default)
    {
        var clinic = await clinicalDbContext.Clinics
            .AsNoTracking()
            .Where(c => c.Id == clinicId && !c.IsDeleted)
            .Select(c => new { c.Name })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        if (clinic is null)
            return null;

        var tickets = await clinicalDbContext.CasualtyTickets
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

        AdminClinicCasualtyTicketDto Map(CasualtyTicket ticket)
        {
            string? name = null;
            if (ticket.PatientId is Guid patientId && patientLookup.TryGetValue(patientId, out var patient))
                name = $"{patient.FirstName} {patient.LastName}".Trim();

            return new AdminClinicCasualtyTicketDto
            {
                Id = ticket.Id,
                PatientId = ticket.PatientId,
                PatientName = name,
                QueueCode = ticket.QueueCode,
                TriageLevel = ticket.TriageLevel,
                ChiefComplaint = ticket.ChiefComplaint,
                Status = ticket.Status,
                ArrivedAt = ticket.ArrivedAt,
                CalledAt = ticket.CalledAt,
                CompletedAt = ticket.CompletedAt
            };
        }

        var waiting = tickets
            .Where(t => t.Status == "Waiting")
            .OrderBy(t => TriageRank(t.TriageLevel))
            .ThenBy(t => t.ArrivedAt)
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

        return new AdminClinicCasualtyBoardDto
        {
            ClinicName = clinic.Name,
            WaitingCount = waiting.Count,
            CalledCount = called.Count,
            Waiting = waiting,
            Called = called,
            Recent = recent
        };
    }

    public async Task<CasualtyDisplayBoardDto?> GetDisplayBoardAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var trimmed = token?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
            return null;

        var clinic = await clinicalDbContext.Clinics
            .AsNoTracking()
            .Where(c => c.CasualtyDisplayToken == trimmed && c.IsActive && !c.IsDeleted)
            .Select(c => new { c.Id, c.Name })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        if (clinic is null)
            return null;

        var open = await clinicalDbContext.CasualtyTickets
            .AsNoTracking()
            .Where(t => t.ClinicId == clinic.Id && (t.Status == "Waiting" || t.Status == "Called"))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var nowServing = open
            .Where(t => t.Status == "Called" && t.CalledAt.HasValue)
            .OrderByDescending(t => t.CalledAt)
            .Select(t => new CasualtyDisplayTicketDto
            {
                QueueCode = t.QueueCode,
                TriageLevel = t.TriageLevel
            })
            .FirstOrDefault();

        var waiting = open
            .Where(t => t.Status == "Waiting")
            .OrderBy(t => TriageRank(t.TriageLevel))
            .ThenBy(t => t.ArrivedAt)
            .Take(8)
            .Select(t => new CasualtyDisplayTicketDto
            {
                QueueCode = t.QueueCode,
                TriageLevel = t.TriageLevel
            })
            .ToList();

        return new CasualtyDisplayBoardDto
        {
            ClinicName = clinic.Name,
            NowServing = nowServing,
            Waiting = waiting
        };
    }

    private static int TriageRank(string level) => level switch
    {
        "Red" => 0,
        "Orange" => 1,
        "Yellow" => 2,
        _ => 3
    };
}
