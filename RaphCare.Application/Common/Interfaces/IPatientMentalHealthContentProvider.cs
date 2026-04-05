using RaphCare.Application.Features.PatientMentalHealth.DTOs;

namespace RaphCare.Application.Common.Interfaces;

/// <summary>Supplies configurable copy for the patient mental health hub (<c>api/patient/mental-health/content</c>).</summary>
public interface IPatientMentalHealthContentProvider
{
    /// <summary>Returns insight and disclaimer strings for the authenticated patient (no PHI).</summary>
    Task<PatientMentalHealthContentDto> GetContentAsync(CancellationToken cancellationToken = default);
}
