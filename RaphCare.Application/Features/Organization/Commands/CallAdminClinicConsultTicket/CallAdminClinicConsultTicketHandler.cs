using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Organization.Commands.CallAdminClinicConsultTicket;

public sealed class CallAdminClinicConsultTicketHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<ConsultTicket> ticketRepository,
    IRepository<Appointment> appointmentRepository,
    IRepository<Patient> patientRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CallAdminClinicConsultTicketCommand, AdminClinicConsultTicketDto?>
{
    public async Task<AdminClinicConsultTicketDto?> Handle(
        CallAdminClinicConsultTicketCommand request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can call a consult code.",
                cancellationToken)
            .ConfigureAwait(false);

        var ticket = await ticketRepository.GetByIdAsync(request.TicketId, cancellationToken).ConfigureAwait(false);
        if (ticket is null || ticket.ClinicId != request.ClinicId)
            return null;

        if (ticket.Status != "Waiting")
            throw new BusinessRuleException("Only a waiting consult ticket can be called.");

        ticket.Status = "Called";
        ticket.CalledAt = clock.UtcNow;
        await ticketRepository.UpdateAsync(ticket, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await MapAsync(ticket, appointmentRepository, patientRepository, cancellationToken)
            .ConfigureAwait(false);
    }

    internal static async Task<AdminClinicConsultTicketDto> MapAsync(
        ConsultTicket ticket,
        IRepository<Appointment> appointmentRepository,
        IRepository<Patient> patientRepository,
        CancellationToken cancellationToken)
    {
        string? patientName = null;
        if (ticket.PatientId is Guid patientId)
        {
            var patient = await patientRepository.GetByIdAsync(patientId, cancellationToken).ConfigureAwait(false);
            if (patient is not null)
                patientName = $"{patient.FirstName} {patient.LastName}".Trim();
        }

        DateTime? scheduledStart = null;
        if (ticket.AppointmentId is Guid appointmentId)
        {
            var appointment = await appointmentRepository.GetByIdAsync(appointmentId, cancellationToken)
                .ConfigureAwait(false);
            scheduledStart = appointment?.ScheduledStart;
        }

        return new AdminClinicConsultTicketDto
        {
            Id = ticket.Id,
            AppointmentId = ticket.AppointmentId,
            PatientId = ticket.PatientId,
            PatientName = patientName,
            ProviderId = ticket.ProviderId,
            QueueCode = ticket.QueueCode,
            Status = ticket.Status,
            ArrivedAt = ticket.ArrivedAt,
            CalledAt = ticket.CalledAt,
            CompletedAt = ticket.CompletedAt,
            ScheduledStart = scheduledStart
        };
    }
}
