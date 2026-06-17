using MediatR;

namespace RaphCare.Application.Features.PatientTelehealth.Commands.SendTelehealthSessionSms;

public class SendTelehealthSessionSmsCommand : IRequest<Unit>
{
    public Guid TeleSessionId { get; set; }
}
