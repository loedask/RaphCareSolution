using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.CallAdminClinicConsultTicket;

public sealed class CallAdminClinicConsultTicketValidator
    : AbstractValidator<CallAdminClinicConsultTicketCommand>
{
    public CallAdminClinicConsultTicketValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.TicketId).NotEmpty();
    }
}
