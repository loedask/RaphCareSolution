using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Organization.Commands.StartAdminClinicVisit;

public sealed class StartAdminClinicVisitHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Appointment> appointmentRepository,
    IRepository<Visit> visitRepository,
    IRepository<Patient> patientRepository,
    IRepository<Provider> providerRepository,
    IRepository<AppointmentConsent> consentRepository,
    IProfessionalUserLookupService professionalUserLookupService,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<StartAdminClinicVisitCommand, AdminClinicVisitDetailDto?>
{
    public async Task<AdminClinicVisitDetailDto?> Handle(
        StartAdminClinicVisitCommand request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.CanDocumentVisitsAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only a doctor or hospital administrator can start visits.");

        var appointment = await appointmentRepository
            .GetByIdAsync(request.AppointmentId, cancellationToken)
            .ConfigureAwait(false);

        if (appointment is null || appointment.ClinicId != request.ClinicId)
            return null;

        if (appointment.IsCancelled)
            throw new BusinessRuleException("Cannot start a visit for a cancelled appointment.");

        var existingOpen = await visitRepository.SearchAsync(
            q => q.Where(v =>
                v.AppointmentId == appointment.Id
                && v.Status != "Completed"
                && v.Status != "Cancelled"),
            1,
            1,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        if (existingOpen.Items.Count > 0)
            return await MapVisitAsync(existingOpen.Items[0], cancellationToken).ConfigureAwait(false);

        var visit = new Visit
        {
            ClinicId = appointment.ClinicId,
            AppointmentId = appointment.Id,
            PatientId = appointment.PatientId,
            ProviderId = appointment.ProviderId,
            VisitStart = clock.UtcNow,
            VisitType = appointment.Type,
            Status = "InProgress",
            Summary = string.IsNullOrWhiteSpace(request.Summary) ? null : request.Summary.Trim()
        };

        await visitRepository.AddAsync(visit, cancellationToken).ConfigureAwait(false);
        appointment.Status = "InProgress";
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await MapVisitAsync(visit, cancellationToken).ConfigureAwait(false);
    }

    private async Task<AdminClinicVisitDetailDto> MapVisitAsync(Visit visit, CancellationToken cancellationToken)
    {
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

        var consentPage = await consentRepository.SearchAsync(
            q => q.Where(c => c.AppointmentId == visit.AppointmentId),
            1,
            1,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        var consent = consentPage.Items.Count > 0 ? consentPage.Items[0] : null;

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
            ConsentSigned = consent is not null,
            ConsentSignedAt = consent?.SignedAt,
            Vitals = Array.Empty<AdminClinicVisitVitalDto>()
        };
    }
}
