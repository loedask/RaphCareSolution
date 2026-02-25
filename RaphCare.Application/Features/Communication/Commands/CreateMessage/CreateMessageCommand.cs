using MediatR;

namespace RaphCare.Application.Features.Communication.Commands.CreateMessage;

public class CreateMessageCommand : IRequest<Guid>
{
    public Guid ClinicId { get; set; }
    public string RecipientUserId { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
}
