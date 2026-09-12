using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.CompleteAdminClinicConsultTicket;

public sealed class CompleteAdminClinicConsultTicketValidator
    : AbstractValidator<CompleteAdminClinicConsultTicketCommand>
{
    public CompleteAdminClinicConsultTicketValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.TicketId).NotEmpty();
    }
}
