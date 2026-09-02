using System.Globalization;
using System.Text;
using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Organization.Commands.DraftAdminClinicDischargeSummary;

public sealed class DraftAdminClinicDischargeSummaryHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<InpatientAdmission> admissionRepository,
    IRepository<InpatientObservation> observationRepository,
    IAIService aiService)
    : IRequestHandler<DraftAdminClinicDischargeSummaryCommand, AdminClinicDischargeSummaryDraftDto?>
{
    private const int MaxDraftLength = 4000;

    public async Task<AdminClinicDischargeSummaryDraftDto?> Handle(
        DraftAdminClinicDischargeSummaryCommand request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService, clinicStaffMembershipService, roleAssignmentService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can draft a discharge summary.");

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
        var draft = await aiService.GenerateSummaryAsync(prompt, cancellationToken).ConfigureAwait(false);
        draft = (draft ?? string.Empty).Trim();
        if (draft.Length > MaxDraftLength)
            draft = draft[..MaxDraftLength];

        return new AdminClinicDischargeSummaryDraftDto { DraftText = draft };
    }

    private static string BuildPrompt(InpatientAdmission admission, IReadOnlyList<InpatientObservation> observations)
    {
        var sb = new StringBuilder();
        var culture = CultureInfo.InvariantCulture;
        sb.AppendLine(culture, $"Draft a discharge summary for this inpatient stay.");
        sb.AppendLine(culture, $"Admitted at (UTC): {admission.AdmittedAt:yyyy-MM-dd HH:mm}");
        if (!string.IsNullOrWhiteSpace(admission.Reason))
            sb.AppendLine(culture, $"Admission reason: {admission.Reason.Trim()}");
        if (!string.IsNullOrWhiteSpace(admission.Notes))
            sb.AppendLine(culture, $"Admission notes: {admission.Notes.Trim()}");

        if (observations.Count == 0)
        {
            sb.AppendLine("Ward notes: none recorded.");
            return sb.ToString();
        }

        sb.AppendLine("Ward notes (oldest first):");
        foreach (var note in observations)
        {
            sb.Append(culture, $"- {note.RecordedAt:yyyy-MM-dd HH:mm} UTC: {note.Note}");
            var vitals = new List<string>();
            if (note.HeartRate is decimal hr)
                vitals.Add(string.Create(culture, $"HR {hr}"));
            if (note.TemperatureCelsius is decimal temp)
                vitals.Add(string.Create(culture, $"Temp {temp} C"));
            if (note.OxygenSaturation is decimal spo2)
                vitals.Add(string.Create(culture, $"SpO2 {spo2}%"));
            if (note.SystolicBp is decimal sys && note.DiastolicBp is decimal dia)
                vitals.Add(string.Create(culture, $"BP {sys}/{dia}"));
            if (vitals.Count > 0)
                sb.Append(culture, $" ({string.Join(", ", vitals)})");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
