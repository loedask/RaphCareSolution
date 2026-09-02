using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Persistence;

public sealed class AdminClinicTheatreQueryService(ClinicalDbContext clinicalDbContext)
    : IAdminClinicTheatreQueryService
{
    public async Task<AdminClinicTheatreBoardDto?> GetBoardAsync(
        Guid clinicId,
        DateTime dayUtc,
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

        var day = DateTime.SpecifyKind(dayUtc.Date, DateTimeKind.Utc);
        var nextDay = day.AddDays(1);

        var cases = await clinicalDbContext.TheatreCases
            .AsNoTracking()
            .Where(c => c.ClinicId == clinicId
                        && c.ScheduledStart >= day
                        && c.ScheduledStart < nextDay)
            .OrderBy(c => c.ScheduledStart)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var patientIds = cases.Select(c => c.PatientId).Distinct().ToList();
        var patients = await clinicalDbContext.Patients
            .AsNoTracking()
            .Where(p => patientIds.Contains(p.Id))
            .Select(p => new { p.Id, p.FirstName, p.LastName })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var patientLookup = patients.ToDictionary(p => p.Id);

        AdminClinicTheatreCaseDto Map(TheatreCase theatreCase)
        {
            patientLookup.TryGetValue(theatreCase.PatientId, out var patient);
            var name = patient is null
                ? "Patient"
                : $"{patient.FirstName} {patient.LastName}".Trim();
            return new AdminClinicTheatreCaseDto
            {
                Id = theatreCase.Id,
                PatientId = theatreCase.PatientId,
                PatientName = name,
                ScheduledStart = theatreCase.ScheduledStart,
                ScheduledEnd = theatreCase.ScheduledEnd,
                ProcedureName = theatreCase.ProcedureName,
                TheatreName = theatreCase.TheatreName,
                SurgeonName = theatreCase.SurgeonName,
                Status = theatreCase.Status,
                Notes = theatreCase.Notes
            };
        }

        var mapped = cases.Select(Map).ToList();
        return new AdminClinicTheatreBoardDto
        {
            ClinicName = clinic.Name,
            DayUtc = day,
            ScheduledCount = mapped.Count(c => c.Status == "Scheduled"),
            InProgressCount = mapped.Count(c => c.Status == "InProgress"),
            CompletedCount = mapped.Count(c => c.Status == "Completed"),
            Cases = mapped
        };
    }
}
