using RaphCare.Client.Contracts;
using RaphCare.Client.Models.MentalHealth;

namespace RaphCare.Client.Contracts.Interfaces;

/// <summary>Patient mental health hub (<c>api/patient/mental-health</c>).</summary>
public interface IPatientMentalHealthService
{
    Task<Response<PatientMentalHealthContentViewModel>> GetContentAsync(CancellationToken cancellationToken = default);

    Task<Response<Guid>> LogMoodCheckInAsync(int moodScore, string? notes, CancellationToken cancellationToken = default);
}
