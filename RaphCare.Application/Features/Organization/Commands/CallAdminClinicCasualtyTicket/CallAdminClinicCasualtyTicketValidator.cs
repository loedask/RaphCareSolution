using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.CallAdminClinicCasualtyTicket;

public sealed class CallAdminClinicCasualtyTicketValidator : AbstractValidator<CallAdminClinicCasualtyTicketCommand>
{
    public CallAdminClinicCasualtyTicketValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.TicketId).NotEmpty();
    }
}
