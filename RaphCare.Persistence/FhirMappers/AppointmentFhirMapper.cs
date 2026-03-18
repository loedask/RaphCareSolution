using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Interoperability.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Persistence.FhirMappers;

/// <summary>
/// Minimal FHIR-shaped Appointment mapper (phase 1).
/// </summary>
public class AppointmentFhirMapper : IAppointmentFhirMapper
{
    public Task<FhirAppointmentDto> MapToDtoAsync(Appointment appointment, CancellationToken ct)
    {
        var status = string.IsNullOrWhiteSpace(appointment.Status) ? null : appointment.Status;

        var dto = new FhirAppointmentDto
        {
            Id = appointment.Id.ToString(),
            Status = status,
            Start = appointment.ScheduledStart,
            End = appointment.ScheduledEnd,
            Comment = string.IsNullOrWhiteSpace(appointment.Reason) ? null : appointment.Reason
        };

        // Phase 1 participant: include patient actor reference only.
        // Provider mapping is deferred because this project phase 1 does not export a FHIR Practitioner reference mapping.
        if (appointment.PatientId != Guid.Empty)
        {
            dto.Participant.Add(new FhirAppointmentParticipantDto
            {
                Actor = new FhirReferenceDto { Reference = $"Patient/{appointment.PatientId}" },
                Status = status
            });
        }

        return Task.FromResult(dto);
    }
}

