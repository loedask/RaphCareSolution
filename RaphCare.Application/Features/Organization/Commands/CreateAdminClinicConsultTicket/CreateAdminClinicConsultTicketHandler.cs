using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicConsultTicket;

public sealed class CreateAdminClinicConsultTicketHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<ConsultTicket> ticketRepository,
    IRepository<Appointment> appointmentRepository,
    IRepository<Patient> patientRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAdminClinicConsultTicketCommand, AdminClinicConsultTicketDto?>
{
    public async Task<AdminClinicConsultTicketDto?> Handle(
        CreateAdminClinicConsultTicketCommand request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can add a consult ticket.",
                cancellationToken)
            .ConfigureAwait(false);

        var appointment = await appointmentRepository
            .GetByIdAsync(request.AppointmentId, cancellationToken)
            .ConfigureAwait(false);
        if (appointment is null || appointment.ClinicId != request.ClinicId)
            return null;

        if (appointment.IsCancelled)
            throw new BusinessRuleException("A cancelled appointment cannot join the consult queue.");

        if (appointment.Status is not ("Scheduled" or "InProgress"))
            throw new BusinessRuleException("Only scheduled or in-progress appointments can join the consult queue.");

        var openForAppointment = await ticketRepository.SearchAsync(
                q => q.Where(t =>
                    t.ClinicId == request.ClinicId
                    && t.AppointmentId == appointment.Id
                    && (t.Status == "Waiting" || t.Status == "Called")),
                1,
                1,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (openForAppointment.TotalCount > 0)
            throw new BusinessRuleException("This appointment is already on the consult queue.");

        string? patientName = null;
        var patient = await patientRepository.GetByIdAsync(appointment.PatientId, cancellationToken)
            .ConfigureAwait(false);
        if (patient is not null)
            patientName = $"{patient.FirstName} {patient.LastName}".Trim();

        var ticket = new ConsultTicket
        {
            ClinicId = request.ClinicId,
            AppointmentId = appointment.Id,
            PatientId = appointment.PatientId,
            ProviderId = appointment.ProviderId,
            QueueCode = await ConsultQueueCode.AllocateAsync(ticketRepository, cancellationToken)
                .ConfigureAwait(false),
            Status = "Waiting",
            ArrivedAt = clock.UtcNow,
            CreatedByApplicationUserId = currentUserService.CurrentUserId
        };

        await ticketRepository.AddAsync(ticket, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

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
            ScheduledStart = appointment.ScheduledStart
        };
    }
}
