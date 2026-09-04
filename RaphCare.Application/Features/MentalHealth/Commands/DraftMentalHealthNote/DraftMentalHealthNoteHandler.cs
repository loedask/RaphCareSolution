using System.Globalization;
using System.Text;
using MediatR;
using Microsoft.Extensions.Logging;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.MentalHealth.DTOs;
using RaphCare.Application.Features.Organization;
using RaphCare.Domain.MentalHealth;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.MentalHealth.Commands.DraftMentalHealthNote;

/// <summary>
/// Builds a redacted prompt from assessment scores and mood scores, then requests an AI therapy note draft.
/// Audits clinic, patient, user, and prompt length only (never the prompt body).
/// </summary>
public sealed partial class DraftMentalHealthNoteHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IPatientClinicAccessService patientClinicAccessService,
    IRepository<Clinic> clinicRepository,
    IRepository<MentalHealthAssessment> assessmentRepository,
    IRepository<MoodLog> moodRepository,
    IAIService aiService,
    ILogger<DraftMentalHealthNoteHandler> logger)
    : IRequestHandler<DraftMentalHealthNoteCommand, MentalHealthNoteDraftDto?>
{
    private const int MaxDraftLength = 4000;

    public async Task<MentalHealthNoteDraftDto?> Handle(
        DraftMentalHealthNoteCommand request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can draft a mental health note.",
                cancellationToken)
            .ConfigureAwait(false);

        var clinic = await clinicRepository.GetByIdAsync(request.ClinicId, cancellationToken).ConfigureAwait(false);
        if (clinic is null || clinic.IsDeleted)
            return null;

        if (!clinic.AllowAiMentalHealthNotes)
        {
            LogAiMentalHealthNoteDenied(request.ClinicId, request.PatientId, currentUserService.CurrentUserId);
            throw new BusinessRuleException(
                "AI mental health note drafting is turned off for this hospital. Write the note by hand, or turn on Allow AI mental health notes in hospital profile.");
        }

        if (!await patientClinicAccessService
                .HasClinicAccessAsync(request.PatientId, request.ClinicId, cancellationToken)
                .ConfigureAwait(false))
            throw new BusinessRuleException("Patient does not have access to this hospital.");

        var assessments = await assessmentRepository.SearchAsync(
                q => q
                    .Where(a => a.ClinicId == request.ClinicId && a.PatientId == request.PatientId)
                    .OrderByDescending(a => a.ConductedAt),
                1,
                5,
                applyDefaultIdOrdering: false,
                cancellationToken)
            .ConfigureAwait(false);

        var moods = await moodRepository.SearchAsync(
                q => q
                    .Where(m => m.PatientId == request.PatientId)
                    .OrderByDescending(m => m.LoggedAt),
                1,
                10,
                applyDefaultIdOrdering: false,
                cancellationToken)
            .ConfigureAwait(false);

        var prompt = BuildPrompt(assessments.Items, moods.Items);
        LogAiMentalHealthNoteRequested(
            request.ClinicId,
            request.PatientId,
            currentUserService.CurrentUserId,
            prompt.Length);

        var draft = await aiService.GenerateSummaryAsync(prompt, cancellationToken).ConfigureAwait(false);
        draft = (draft ?? string.Empty).Trim();
        if (draft.Length > MaxDraftLength)
            draft = draft[..MaxDraftLength];

        return new MentalHealthNoteDraftDto { DraftText = draft };
    }

    /// <summary>
    /// Sends assessment type/score/severity and mood scores only.
    /// Omits free-text mood notes and questionnaire item text.
    /// </summary>
    public static string BuildPrompt(
        IReadOnlyList<MentalHealthAssessment> assessments,
        IReadOnlyList<MoodLog> moods)
    {
        var sb = new StringBuilder();
        var culture = CultureInfo.InvariantCulture;
        sb.AppendLine("Draft a brief therapy progress note for clinician review.");
        sb.AppendLine("Use only the structured scores below. Do not invent diagnoses. Staff will edit before saving.");

        if (assessments.Count == 0)
            sb.AppendLine("Recent assessments: none.");
        else
        {
            sb.AppendLine("Recent assessments (newest first):");
            foreach (var a in assessments)
            {
                sb.AppendLine(
                    culture,
                    $"- {a.AssessmentType} on {a.ConductedAt:yyyy-MM-dd}: severity {a.SeverityLevel}, score {a.TotalScore}");
            }
        }

        if (moods.Count == 0)
            sb.AppendLine("Recent mood check-ins: none.");
        else
        {
            sb.AppendLine("Recent mood check-ins (0 great to 3 low; notes omitted):");
            foreach (var m in moods)
            {
                sb.AppendLine(
                    culture,
                    $"- {m.LoggedAt:yyyy-MM-dd}: score {m.MoodScore}{(m.IsFlagged ? " (flagged)" : string.Empty)}");
            }
        }

        return sb.ToString();
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "AI mental health note draft requested for clinic {ClinicId}, patient {PatientId}, user {UserId}; prompt length {PromptLength}.")]
    private partial void LogAiMentalHealthNoteRequested(Guid clinicId, Guid patientId, Guid? userId, int promptLength);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "AI mental health note draft denied for clinic {ClinicId}, patient {PatientId}, user {UserId}; hospital toggle off.")]
    private partial void LogAiMentalHealthNoteDenied(Guid clinicId, Guid patientId, Guid? userId);
}
