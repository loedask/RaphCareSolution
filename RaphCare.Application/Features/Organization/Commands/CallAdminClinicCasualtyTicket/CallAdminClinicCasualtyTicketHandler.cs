using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Organization.Commands.CallAdminClinicCasualtyTicket;

public sealed class CallAdminClinicCasualtyTicketHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<CasualtyTicket> ticketRepository,
    IRepository<Patient> patientRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CallAdminClinicCasualtyTicketCommand, AdminClinicCasualtyTicketDto?>
{
    public async Task<AdminClinicCasualtyTicketDto?> Handle(
        CallAdminClinicCasualtyTicketCommand request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can call a casualty code.",
                cancellationToken)
            .ConfigureAwait(false);

        var ticket = await ticketRepository.GetByIdAsync(request.TicketId, cancellationToken).ConfigureAwait(false);
        if (ticket is null || ticket.ClinicId != request.ClinicId)
            return null;

        if (ticket.Status != "Waiting")
            throw new BusinessRuleException("Only a waiting casualty ticket can be called.");

        ticket.Status = "Called";
        ticket.CalledAt = clock.UtcNow;
        await ticketRepository.UpdateAsync(ticket, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await MapAsync(ticket, patientRepository, cancellationToken).ConfigureAwait(false);
    }

    internal static async Task<AdminClinicCasualtyTicketDto> MapAsync(
        CasualtyTicket ticket,
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

        return new AdminClinicCasualtyTicketDto
        {
            Id = ticket.Id,
            PatientId = ticket.PatientId,
            PatientName = patientName,
            QueueCode = ticket.QueueCode,
            TriageLevel = ticket.TriageLevel,
            ChiefComplaint = ticket.ChiefComplaint,
            Status = ticket.Status,
            ArrivedAt = ticket.ArrivedAt,
            CalledAt = ticket.CalledAt,
            CompletedAt = ticket.CompletedAt
        };
    }
}
