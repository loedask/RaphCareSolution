using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.Commands.CallAdminClinicCasualtyTicket;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Organization.Commands.CompleteAdminClinicCasualtyTicket;

public sealed class CompleteAdminClinicCasualtyTicketHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<CasualtyTicket> ticketRepository,
    IRepository<Patient> patientRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CompleteAdminClinicCasualtyTicketCommand, AdminClinicCasualtyTicketDto?>
{
    public async Task<AdminClinicCasualtyTicketDto?> Handle(
        CompleteAdminClinicCasualtyTicketCommand request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can close a casualty ticket.",
                cancellationToken)
            .ConfigureAwait(false);

        var ticket = await ticketRepository.GetByIdAsync(request.TicketId, cancellationToken).ConfigureAwait(false);
        if (ticket is null || ticket.ClinicId != request.ClinicId)
            return null;

        if (ticket.Status is not ("Waiting" or "Called"))
            throw new BusinessRuleException("This casualty ticket is already closed.");

        ticket.Status = request.Cancel ? "Cancelled" : "Completed";
        ticket.CompletedAt = clock.UtcNow;
        await ticketRepository.UpdateAsync(ticket, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await CallAdminClinicCasualtyTicketHandler
            .MapAsync(ticket, patientRepository, cancellationToken)
            .ConfigureAwait(false);
    }
}
