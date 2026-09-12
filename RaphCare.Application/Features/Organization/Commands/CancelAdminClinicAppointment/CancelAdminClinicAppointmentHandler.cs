using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Appointments;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Organization.Commands.CancelAdminClinicAppointment;

public sealed class CancelAdminClinicAppointmentHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Appointment> appointmentRepository,
    IRepository<AppointmentReminder> appointmentReminderRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CancelAdminClinicAppointmentCommand, bool>
{
    public async Task<bool> Handle(CancelAdminClinicAppointmentCommand request, CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can cancel appointments.");

        var appointment = await appointmentRepository
            .GetByIdAsync(request.AppointmentId, cancellationToken)
            .ConfigureAwait(false);

        if (appointment is null || appointment.ClinicId != request.ClinicId)
            return false;

        if (appointment.IsCancelled)
            return true;

        appointment.IsCancelled = true;
        appointment.Status = "Cancelled";
        await AppointmentReminderPlanner.ClearUnsentAsync(
                appointmentReminderRepository,
                appointment.Id,
                cancellationToken)
            .ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
