namespace RaphCare.Application.Features.PatientMentalHealth.DTOs;

public sealed class PatientMoodCheckInDto
{
    public Guid Id { get; set; }
    public DateTime LoggedAt { get; set; }
    public int MoodScore { get; set; }
}
