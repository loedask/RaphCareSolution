namespace RaphCare.Application.Features.Interoperability.DTOs;

/// <summary>
/// Minimal FHIR (Fast Healthcare Interoperability Resources)-shaped Appointment.participant element.
/// </summary>
public class FhirAppointmentParticipantDto
{
    public FhirReferenceDto Actor { get; set; } = new();

    public string? Status { get; set; }
}

