using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicCasualtyTicket;

public sealed class CreateAdminClinicCasualtyTicketHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<CasualtyTicket> ticketRepository,
    IRepository<Patient> patientRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAdminClinicCasualtyTicketCommand, AdminClinicCasualtyTicketDto?>
{
    public async Task<AdminClinicCasualtyTicketDto?> Handle(
        CreateAdminClinicCasualtyTicketCommand request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can add a casualty ticket.",
                cancellationToken)
            .ConfigureAwait(false);

        string? patientName = null;
        if (request.PatientId is Guid patientId)
        {
            var patient = await patientRepository.GetByIdAsync(patientId, cancellationToken).ConfigureAwait(false);
            if (patient is null)
                return null;
            patientName = $"{patient.FirstName} {patient.LastName}".Trim();
        }

        var level = NormalizeLevel(request.TriageLevel);
        var ticket = new CasualtyTicket
        {
            ClinicId = request.ClinicId,
            PatientId = request.PatientId,
            QueueCode = await CasualtyQueueCode.AllocateAsync(ticketRepository, cancellationToken)
                .ConfigureAwait(false),
            TriageLevel = level,
            ChiefComplaint = string.IsNullOrWhiteSpace(request.ChiefComplaint)
                ? null
                : request.ChiefComplaint.Trim(),
            Status = "Waiting",
            ArrivedAt = clock.UtcNow,
            CreatedByApplicationUserId = currentUserService.CurrentUserId
        };

        await ticketRepository.AddAsync(ticket, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new AdminClinicCasualtyTicketDto
        {
            Id = ticket.Id,
            PatientId = ticket.PatientId,
            PatientName = patientName,
            QueueCode = ticket.QueueCode,
            TriageLevel = ticket.TriageLevel,
            ChiefComplaint = ticket.ChiefComplaint,
            Status = ticket.Status,
            ArrivedAt = ticket.ArrivedAt
        };
    }

    private static string NormalizeLevel(string level) => level.Trim() switch
    {
        var value when value.Equals("Red", StringComparison.OrdinalIgnoreCase) => "Red",
        var value when value.Equals("Orange", StringComparison.OrdinalIgnoreCase) => "Orange",
        var value when value.Equals("Yellow", StringComparison.OrdinalIgnoreCase) => "Yellow",
        _ => "Green"
    };
}
