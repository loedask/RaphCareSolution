using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Interoperability.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Persistence.FhirMappers;

/// <summary>
/// Minimal FHIR (Fast Healthcare Interoperability Resources)-shaped Encounter mapper (phase 1).
/// </summary>
public class EncounterFhirMapper : IEncounterFhirMapper
{
    public Task<FhirEncounterDto> MapToDtoAsync(Visit visit, CancellationToken ct)
    {
        var dto = new FhirEncounterDto
        {
            Id = visit.Id.ToString(),
            Status = string.IsNullOrWhiteSpace(visit.Status) ? null : visit.Status,
            Subject = new FhirReferenceDto { Reference = $"Patient/{visit.PatientId}" },
            ServiceProvider = new FhirReferenceDto { Reference = $"Organization/{visit.ClinicId}" },
            Period = new FhirPeriodDto
            {
                Start = visit.VisitStart,
                End = visit.VisitEnd
            }
        };

        // appointment reference is optional in phase 1; include only if it looks linked.
        if (visit.AppointmentId != Guid.Empty)
        {
            dto.Appointment = new FhirReferenceDto { Reference = $"Appointment/{visit.AppointmentId}" };
        }

        return Task.FromResult(dto);
    }
}

