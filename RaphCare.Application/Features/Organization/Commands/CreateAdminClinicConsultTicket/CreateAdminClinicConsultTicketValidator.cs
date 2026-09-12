using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicConsultTicket;

public sealed class CreateAdminClinicConsultTicketValidator
    : AbstractValidator<CreateAdminClinicConsultTicketCommand>
{
    public CreateAdminClinicConsultTicketValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.AppointmentId).NotEmpty();
    }
}
