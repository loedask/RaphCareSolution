using MediatR;

namespace RaphCare.Application.Features.Communication.Commands.UpdateMessage;

public class UpdateMessageCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public DateTime? SentAt { get; set; }
    public bool? IsSent { get; set; }
}
