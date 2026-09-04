using System.Globalization;
using System.Text;
using MediatR;
using Microsoft.Extensions.Logging;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.DraftAdminClinicDischargeSummary;

/// <summary>
/// Builds a redacted stay prompt and requests an AI discharge draft.
/// Audits clinic, admission, user, and prompt length only (never the prompt body).
/// </summary>
public sealed partial class DraftAdminClinicDischargeSummaryHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Clinic> clinicRepository,
    IRepository<InpatientAdmission> admissionRepository,
    IRepository<InpatientObservation> observationRepository,
    IAIService aiService,
    ILogger<DraftAdminClinicDischargeSummaryHandler> logger)
    : IRequestHandler<DraftAdminClinicDischargeSummaryCommand, AdminClinicDischargeSummaryDraftDto?>
{
    private const int MaxDraftLength = 4000;
    private const int MaxReasonLength = 200;

    public async Task<AdminClinicDischargeSummaryDraftDto?> Handle(
        DraftAdminClinicDischargeSummaryCommand request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService, clinicStaffMembershipService, roleAssignmentService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can draft a discharge summary.");

        var clinic = await clinicRepository.GetByIdAsync(request.ClinicId, cancellationToken).ConfigureAwait(false);
        if (clinic is null || clinic.IsDeleted)
            return null;

        if (!clinic.AllowAiDischargeDraft)
        {
            LogAiDischargeDraftDenied(request.ClinicId, request.AdmissionId, currentUserService.CurrentUserId);
            throw new BusinessRuleException(
                "AI discharge drafting is turned off for this hospital. Write the summary by hand, or turn on Allow AI discharge draft in hospital profile.");
        }

        var admission = await admissionRepository.GetByIdAsync(request.AdmissionId, cancellationToken).ConfigureAwait(false);
        if (admission is null || admission.ClinicId != request.ClinicId)
            return null;

        if (admission.Status != "Admitted")
            throw new BusinessRuleException("Only an active stay can get a discharge summary draft.");

        var observationsPage = await observationRepository.SearchAsync(
            q => q.Where(o => o.AdmissionId == admission.Id).OrderBy(o => o.RecordedAt),
            1,
            50,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var prompt = BuildPrompt(admission, observationsPage.Items);
        LogAiDischargeDraftRequested(
            request.ClinicId,
            request.AdmissionId,
            currentUserService.CurrentUserId,
            prompt.Length);

        var draft = await aiService.GenerateSummaryAsync(prompt, cancellationToken).ConfigureAwait(false);
        draft = (draft ?? string.Empty).Trim();
        if (draft.Length > MaxDraftLength)
            draft = draft[..MaxDraftLength];

        return new AdminClinicDischargeSummaryDraftDto { DraftText = draft };
    }

    /// <summary>
    /// Sends admission reason (truncated) and ward vitals only.
    /// Omits admission notes and free-text ward notes so narrative PHI is not sent to the model.
    /// </summary>
    internal static string BuildPrompt(InpatientAdmission admission, IReadOnlyList<InpatientObservation> observations)
    {
        var sb = new StringBuilder();
        var culture = CultureInfo.InvariantCulture;
        sb.AppendLine(culture, $"Draft a discharge summary for this inpatient stay.");
        sb.AppendLine(culture, $"Admitted at (UTC): {admission.AdmittedAt:yyyy-MM-dd HH:mm}");
        if (!string.IsNullOrWhiteSpace(admission.Reason))
        {
            var reason = admission.Reason.Trim();
            if (reason.Length > MaxReasonLength)
                reason = reason[..MaxReasonLength];
            sb.AppendLine(culture, $"Admission reason: {reason}");
        }

        if (observations.Count == 0)
        {
            sb.AppendLine("Ward vitals: none recorded.");
            return sb.ToString();
        }

        sb.AppendLine("Ward vitals (oldest first; free-text notes omitted):");
        foreach (var note in observations)
        {
            var vitals = new List<string>();
            if (note.HeartRate is decimal hr)
                vitals.Add(string.Create(culture, $"HR {hr}"));
            if (note.TemperatureCelsius is decimal temp)
                vitals.Add(string.Create(culture, $"Temp {temp} C"));
            if (note.OxygenSaturation is decimal spo2)
                vitals.Add(string.Create(culture, $"SpO2 {spo2}%"));
            if (note.SystolicBp is decimal sys && note.DiastolicBp is decimal dia)
                vitals.Add(string.Create(culture, $"BP {sys}/{dia}"));

            if (vitals.Count == 0)
                sb.AppendLine(culture, $"- {note.RecordedAt:yyyy-MM-dd HH:mm} UTC: (no vitals)");
            else
                sb.AppendLine(culture, $"- {note.RecordedAt:yyyy-MM-dd HH:mm} UTC: {string.Join(", ", vitals)}");
        }

        return sb.ToString();
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "AI discharge draft requested for clinic {ClinicId}, admission {AdmissionId}, user {UserId}; prompt length {PromptLength}.")]
    private partial void LogAiDischargeDraftRequested(Guid clinicId, Guid admissionId, Guid? userId, int promptLength);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "AI discharge draft denied for clinic {ClinicId}, admission {AdmissionId}, user {UserId}; hospital toggle off.")]
    private partial void LogAiDischargeDraftDenied(Guid clinicId, Guid admissionId, Guid? userId);
}
