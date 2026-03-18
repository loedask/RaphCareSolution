namespace RaphCare.Application.Features.Interoperability.DTOs;

/// <summary>
/// Minimal FHIR-shaped Appointment.participant element.
/// </summary>
public class FhirAppointmentParticipantDto
{
    public FhirReferenceDto Actor { get; set; } = new();

    public string? Status { get; set; }
}

