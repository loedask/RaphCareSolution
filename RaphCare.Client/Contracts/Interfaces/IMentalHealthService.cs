using RaphCare.Client.Contracts;
using RaphCare.Client.Models.Api;
using RaphCare.Client.Models.MentalHealth;

namespace RaphCare.Client.Contracts.Interfaces;

/// <summary>Staff mental health assessments, therapy, and care plans (<c>api/MentalHealth</c>).</summary>
public interface IMentalHealthService
{
    Task<Response<PagedApiResult<MentalHealthAssessmentListItem>>> GetAssessmentsAsync(
        Guid clinicId,
        Guid? patientId = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<Response<MentalHealthAssessmentDetail>> GetAssessmentByIdAsync(
        Guid clinicId,
        Guid assessmentId,
        CancellationToken cancellationToken = default);

    Task<Response<MentalHealthInstrument>> GetInstrumentAsync(
        string assessmentType = "PHQ-9",
        CancellationToken cancellationToken = default);

    Task<Response<MentalHealthAssessmentDetail>> CreateAssessmentAsync(
        CreateMentalHealthAssessmentRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<PagedApiResult<TherapySessionListItem>>> GetTherapySessionsAsync(
        Guid clinicId,
        Guid? patientId = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<Response<TherapySessionListItem>> CreateTherapySessionAsync(
        CreateTherapySessionRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<TherapyNoteItem>> AddTherapyNoteAsync(
        AddTherapyNoteRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<CrisisFlagItem>> ResolveCrisisFlagAsync(
        ResolveCrisisFlagRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<PagedApiResult<BehavioralCarePlanItem>>> GetBehavioralCarePlansAsync(
        Guid clinicId,
        Guid? patientId = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<Response<BehavioralCarePlanItem>> CreateBehavioralCarePlanAsync(
        CreateBehavioralCarePlanRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<TherapyGoalItem>> CompleteTherapyGoalAsync(
        CompleteTherapyGoalRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<MentalHealthNoteDraft>> DraftMentalHealthNoteAsync(
        DraftMentalHealthNoteRequest request,
        CancellationToken cancellationToken = default);
}
