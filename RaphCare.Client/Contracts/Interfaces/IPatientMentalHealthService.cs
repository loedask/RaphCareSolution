using RaphCare.Client.Contracts;
using RaphCare.Client.Models.Api;
using RaphCare.Client.Models.MentalHealth;

namespace RaphCare.Client.Contracts.Interfaces;

/// <summary>Patient mental health hub (<c>api/patient/mental-health</c>).</summary>
public interface IPatientMentalHealthService
{
    Task<Response<PatientMentalHealthContentViewModel>> GetContentAsync(CancellationToken cancellationToken = default);

    Task<Response<IReadOnlyList<PatientMoodCheckInViewModel>>> GetMyMoodCheckInsAsync(
        int pageSize = 14,
        CancellationToken cancellationToken = default);

    Task<Response<Guid>> LogMoodCheckInAsync(int moodScore, string? notes, CancellationToken cancellationToken = default);

    Task<Response<MentalHealthInstrument>> GetInstrumentAsync(
        string assessmentType = "PHQ-9",
        CancellationToken cancellationToken = default);

    Task<Response<PagedApiResult<MentalHealthAssessmentListItem>>> GetAssessmentsAsync(
        Guid? clinicId = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<Response<MentalHealthAssessmentDetail>> SubmitAssessmentAsync(
        SubmitPatientMentalHealthAssessmentRequest request,
        CancellationToken cancellationToken = default);
}
