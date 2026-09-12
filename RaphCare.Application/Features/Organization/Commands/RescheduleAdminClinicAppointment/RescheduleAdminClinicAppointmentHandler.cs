using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Appointments;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Organization.Commands.RescheduleAdminClinicAppointment;

public sealed class RescheduleAdminClinicAppointmentHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Appointment> appointmentRepository,
    IRepository<AppointmentReminder> appointmentReminderRepository,
    IRepository<Provider> providerRepository,
    IRepository<ProviderSchedule> providerScheduleRepository,
    IRepository<Clinic> clinicRepository,
    IRepository<Patient> patientRepository,
    IProfessionalUserLookupService professionalUserLookupService,
    IDateTimeProvider clock,
    IMediator mediator,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RescheduleAdminClinicAppointmentCommand, AdminClinicAppointmentListItemDto?>
{
    public async Task<AdminClinicAppointmentListItemDto?> Handle(
        RescheduleAdminClinicAppointmentCommand request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can reschedule appointments.");

        var appointment = await appointmentRepository
            .GetByIdAsync(request.AppointmentId, cancellationToken)
            .ConfigureAwait(false);

        if (appointment is null || appointment.ClinicId != request.ClinicId)
            return null;

        if (appointment.IsCancelled)
            throw new BusinessRuleException("Cannot reschedule a cancelled appointment.");

        var providerId = request.ProviderId ?? appointment.ProviderId;
        var provider = await providerRepository.GetByIdAsync(providerId, cancellationToken).ConfigureAwait(false);
        if (provider is null || provider.IsDeleted || provider.ClinicId != request.ClinicId || !provider.IsActive)
            throw new BusinessRuleException("Provider not found for this hospital.");

        var patient = await patientRepository.GetByIdAsync(appointment.PatientId, cancellationToken).ConfigureAwait(false);
        if (patient is null || patient.IsDeleted)
            throw new BusinessRuleException("Patient not found.");

        await AppointmentSchedulingGuard.EnsureNoProviderConflictAsync(
                appointmentRepository,
                request.ClinicId,
                providerId,
                request.ScheduledStart,
                request.ScheduledEnd,
                excludeAppointmentId: appointment.Id,
                cancellationToken)
            .ConfigureAwait(false);

        var schedulesPage = await providerScheduleRepository.SearchAsync(
            q => q.Where(s => s.ProviderId == providerId),
            1,
            50,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);
        var clinic = await clinicRepository.GetByIdAsync(request.ClinicId, cancellationToken).ConfigureAwait(false);
        AppointmentSchedulingGuard.EnsureFitsWeeklySchedule(
            schedulesPage.Items,
            request.ScheduledStart,
            request.ScheduledEnd,
            clinic?.TimeZone);

        appointment.ProviderId = providerId;
        appointment.ScheduledStart = request.ScheduledStart;
        appointment.ScheduledEnd = request.ScheduledEnd;
        if (request.Reason is not null)
            appointment.Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();

        await AppointmentReminderPlanner.ReplaceUnsentAsync(
                appointmentReminderRepository,
                appointment.Id,
                appointment.ScheduledStart,
                clock.UtcNow,
                cancellationToken)
            .ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await AppointmentPatientNotifier.NotifyRescheduledAsync(
                mediator,
                appointment.PatientId,
                clinic?.Name ?? string.Empty,
                appointment.ScheduledStart,
                cancellationToken)
            .ConfigureAwait(false);

        var users = await professionalUserLookupService
            .GetUsersByIdsAsync([provider.ApplicationUserId], cancellationToken)
            .ConfigureAwait(false);
        var user = users.Count > 0 ? users[0] : null;
        var providerName = user is null
            ? "Provider"
            : string.IsNullOrWhiteSpace(user.DisplayName) ? user.Email : user.DisplayName;

        return new AdminClinicAppointmentListItemDto
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            PatientName = $"{patient.FirstName} {patient.LastName}".Trim(),
            ProviderId = providerId,
            ProviderName = providerName ?? "Provider",
            ScheduledStart = appointment.ScheduledStart,
            ScheduledEnd = appointment.ScheduledEnd,
            Type = appointment.Type,
            Status = appointment.Status,
            Reason = appointment.Reason
        };
    }
}
