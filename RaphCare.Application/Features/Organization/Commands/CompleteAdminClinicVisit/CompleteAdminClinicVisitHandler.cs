using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Organization.Commands.CompleteAdminClinicVisit;

public sealed class CompleteAdminClinicVisitHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Visit> visitRepository,
    IRepository<Appointment> appointmentRepository,
    IRepository<VitalSignRecord> vitalRepository,
    IRepository<Patient> patientRepository,
    IRepository<Provider> providerRepository,
    IProfessionalUserLookupService professionalUserLookupService,
    IAdminClinicPatientQueryService adminClinicPatientQueryService,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CompleteAdminClinicVisitCommand, AdminClinicVisitDetailDto?>
{
    public async Task<AdminClinicVisitDetailDto?> Handle(
        CompleteAdminClinicVisitCommand request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.CanDocumentVisitsAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only a doctor or hospital administrator can complete visits.");

        var visit = await visitRepository.GetByIdAsync(request.VisitId, cancellationToken).ConfigureAwait(false);
        if (visit is null || visit.ClinicId != request.ClinicId)
            return null;

        if (visit.Status == "Completed")
            return await MapAsync(visit, cancellationToken).ConfigureAwait(false);

        visit.Status = "Completed";
        visit.VisitEnd = clock.UtcNow;
        if (request.Summary is not null)
            visit.Summary = string.IsNullOrWhiteSpace(request.Summary) ? null : request.Summary.Trim();

        var appointment = await appointmentRepository
            .GetByIdAsync(visit.AppointmentId, cancellationToken)
            .ConfigureAwait(false);
        if (appointment is not null && !appointment.IsCancelled)
            appointment.Status = "Completed";

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return await MapAsync(visit, cancellationToken).ConfigureAwait(false);
    }

    private async Task<AdminClinicVisitDetailDto> MapAsync(Visit visit, CancellationToken cancellationToken)
    {
        var vitalsPage = await vitalRepository.SearchAsync(
            q => q.Where(v => v.VisitId == visit.Id).OrderByDescending(v => v.RecordedAt),
            1,
            100,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var patient = await patientRepository.GetByIdAsync(visit.PatientId, cancellationToken).ConfigureAwait(false);
        var provider = await providerRepository.GetByIdAsync(visit.ProviderId, cancellationToken).ConfigureAwait(false);
        var providerName = "Provider";
        if (provider is not null)
        {
            var users = await professionalUserLookupService
                .GetUsersByIdsAsync([provider.ApplicationUserId], cancellationToken)
                .ConfigureAwait(false);
            var user = users.Count > 0 ? users[0] : null;
            if (user is not null)
                providerName = string.IsNullOrWhiteSpace(user.DisplayName) ? user.Email : user.DisplayName;
        }

        var clinical = await adminClinicPatientQueryService
            .GetVisitClinicalDocumentationAsync(visit.Id, visit.VisitStart, cancellationToken)
            .ConfigureAwait(false);

        return new AdminClinicVisitDetailDto
        {
            Id = visit.Id,
            ClinicId = visit.ClinicId,
            AppointmentId = visit.AppointmentId,
            PatientId = visit.PatientId,
            PatientName = patient is null ? "Patient" : $"{patient.FirstName} {patient.LastName}".Trim(),
            ProviderId = visit.ProviderId,
            ProviderName = providerName ?? "Provider",
            VisitStart = visit.VisitStart,
            VisitEnd = visit.VisitEnd,
            VisitType = visit.VisitType,
            Status = visit.Status,
            Summary = visit.Summary,
            Vitals = vitalsPage.Items.Select(v => new AdminClinicVisitVitalDto
            {
                Id = v.Id,
                Type = v.Type,
                Value = v.Value,
                Unit = v.Unit,
                RecordedAt = v.RecordedAt
            }).ToList(),
            Diagnoses = clinical.Diagnoses,
            Prescriptions = clinical.Prescriptions,
            ClinicalNotes = clinical.ClinicalNotes,
            SoapNotes = clinical.SoapNotes,
            LabResults = clinical.LabResults
        };
    }
}
