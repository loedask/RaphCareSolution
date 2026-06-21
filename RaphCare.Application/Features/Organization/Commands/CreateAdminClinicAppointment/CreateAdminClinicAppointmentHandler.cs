using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicAppointment;

public sealed class CreateAdminClinicAppointmentHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IPatientClinicAccessService patientClinicAccessService,
    IRepository<Appointment> appointmentRepository,
    IRepository<Provider> providerRepository,
    IRepository<Patient> patientRepository,
    IProfessionalUserLookupService professionalUserLookupService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAdminClinicAppointmentCommand, AdminClinicAppointmentListItemDto?>
{
    public async Task<AdminClinicAppointmentListItemDto?> Handle(
        CreateAdminClinicAppointmentCommand request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can book appointments.");

        if (!await patientClinicAccessService
                .HasClinicAccessAsync(request.PatientId, request.ClinicId, cancellationToken)
                .ConfigureAwait(false))
            throw new BusinessRuleException("Patient does not have access to this hospital.");

        var patient = await patientRepository.GetByIdAsync(request.PatientId, cancellationToken).ConfigureAwait(false);
        if (patient is null || patient.IsDeleted)
            throw new BusinessRuleException("Patient not found.");

        var provider = await providerRepository.GetByIdAsync(request.ProviderId, cancellationToken).ConfigureAwait(false);
        if (provider is null || provider.IsDeleted || provider.ClinicId != request.ClinicId || !provider.IsActive)
            throw new BusinessRuleException("Provider not found for this hospital.");

        var appointment = new Appointment
        {
            ClinicId = request.ClinicId,
            PatientId = request.PatientId,
            ProviderId = request.ProviderId,
            ScheduledStart = request.ScheduledStart,
            ScheduledEnd = request.ScheduledEnd,
            Type = request.Type.Trim(),
            Status = "Scheduled",
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim()
        };

        await appointmentRepository.AddAsync(appointment, cancellationToken).ConfigureAwait(false);
        await patientClinicAccessService
            .GrantEncounterAccessAsync(request.PatientId, request.ClinicId, cancellationToken)
            .ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var users = await professionalUserLookupService
            .GetUsersByIdsAsync([provider.ApplicationUserId], cancellationToken)
            .ConfigureAwait(false);
        var user = users.FirstOrDefault();
        var providerName = user is null
            ? "Provider"
            : string.IsNullOrWhiteSpace(user.DisplayName) ? user.Email : user.DisplayName;

        return new AdminClinicAppointmentListItemDto
        {
            Id = appointment.Id,
            PatientId = request.PatientId,
            PatientName = $"{patient.FirstName} {patient.LastName}".Trim(),
            ProviderId = request.ProviderId,
            ProviderName = providerName ?? "Provider",
            ScheduledStart = appointment.ScheduledStart,
            ScheduledEnd = appointment.ScheduledEnd,
            Type = appointment.Type,
            Status = appointment.Status,
            Reason = appointment.Reason
        };
    }
}
