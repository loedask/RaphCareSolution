using System.Text.Json.Serialization;

namespace RaphCare.Application.Features.Interoperability.DTOs;

/// <summary>
/// FHIR-shaped Appointment (phase 1 export, not a full FHIR implementation).
/// </summary>
public class FhirAppointmentDto
{
    [JsonPropertyName("resourceType")]
    public string ResourceType { get; set; } = "Appointment";

    public string Id { get; set; } = string.Empty;

    public string? Status { get; set; }

    public DateTime? Start { get; set; }

    public DateTime? End { get; set; }

    public List<FhirAppointmentParticipantDto> Participant { get; set; } = new();

    // Convenience: we export domain Appointment.Reason as FHIR Appointment.comment.
    public string? Comment { get; set; }
}

