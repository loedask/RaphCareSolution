using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.Commands.CallAdminClinicConsultTicket;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Organization.Commands.CompleteAdminClinicConsultTicket;

public sealed class CompleteAdminClinicConsultTicketHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<ConsultTicket> ticketRepository,
    IRepository<Appointment> appointmentRepository,
    IRepository<Patient> patientRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CompleteAdminClinicConsultTicketCommand, AdminClinicConsultTicketDto?>
{
    public async Task<AdminClinicConsultTicketDto?> Handle(
        CompleteAdminClinicConsultTicketCommand request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can close a consult ticket.",
                cancellationToken)
            .ConfigureAwait(false);

        var ticket = await ticketRepository.GetByIdAsync(request.TicketId, cancellationToken).ConfigureAwait(false);
        if (ticket is null || ticket.ClinicId != request.ClinicId)
            return null;

        if (ticket.Status is not ("Waiting" or "Called"))
            throw new BusinessRuleException("This consult ticket is already closed.");

        ticket.Status = request.Cancel ? "Cancelled" : "Completed";
        ticket.CompletedAt = clock.UtcNow;
        await ticketRepository.UpdateAsync(ticket, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await CallAdminClinicConsultTicketHandler
            .MapAsync(ticket, appointmentRepository, patientRepository, cancellationToken)
            .ConfigureAwait(false);
    }
}
