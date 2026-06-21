using MediatR;
using RaphCare.Application.Features.PatientTelehealth.DTOs;

namespace RaphCare.Application.Features.PatientTelehealth.Queries.GetTelehealthSessionChat;

public sealed class GetTelehealthSessionChatQuery : IRequest<IReadOnlyList<TelehealthChatMessageDto>>
{
    public Guid TeleSessionId { get; set; }
    public int PageSize { get; set; } = 50;
}
