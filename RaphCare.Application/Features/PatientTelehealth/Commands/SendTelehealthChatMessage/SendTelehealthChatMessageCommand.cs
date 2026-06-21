using MediatR;

namespace RaphCare.Application.Features.PatientTelehealth.Commands.SendTelehealthChatMessage;

public sealed class SendTelehealthChatMessageCommand : IRequest<Guid>
{
    public Guid TeleSessionId { get; set; }
    public string Message { get; set; } = string.Empty;
}
