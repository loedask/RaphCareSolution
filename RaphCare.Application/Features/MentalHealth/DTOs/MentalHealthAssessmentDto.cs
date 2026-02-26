namespace RaphCare.Application.Features.MentalHealth.DTOs;

public class MentalHealthAssessmentDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public DateTime AssessedAt { get; set; }
    public string? Summary { get; set; }
}
