namespace RaphCare.Application.Features.MentalHealth.DTOs;

/// <summary>Application-layer DTO for a mental health assessment in list or detail responses.</summary>
public class MentalHealthAssessmentDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public DateTime AssessedAt { get; set; }
    public string? Summary { get; set; }
}
