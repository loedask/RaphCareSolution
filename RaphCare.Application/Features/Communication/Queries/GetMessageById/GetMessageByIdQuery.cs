using MediatR;
using RaphCare.Application.Features.Communication.DTOs;

namespace RaphCare.Application.Features.Communication.Queries.GetMessageById;

public class GetMessageByIdQuery : IRequest<MessageDto>
{
    public Guid Id { get; set; }
}
