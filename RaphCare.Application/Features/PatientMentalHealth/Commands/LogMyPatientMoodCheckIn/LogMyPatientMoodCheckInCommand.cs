using MediatR;

namespace RaphCare.Application.Features.PatientMentalHealth.Commands.LogMyPatientMoodCheckIn;

/// <summary>Records a patient mood check-in (0 = great … 3 = low). Persists to clinical <c>MoodLogs</c>.</summary>
public sealed class LogMyPatientMoodCheckInCommand : IRequest<Guid>
{
    /// <summary>0 = great, 1 = good, 2 = okay, 3 = low.</summary>
    public int MoodScore { get; set; }

    public string? Notes { get; set; }
}
