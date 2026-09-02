using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.CompleteAdminClinicCasualtyTicket;

public sealed class CompleteAdminClinicCasualtyTicketValidator
    : AbstractValidator<CompleteAdminClinicCasualtyTicketCommand>
{
    public CompleteAdminClinicCasualtyTicketValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.TicketId).NotEmpty();
    }
}
