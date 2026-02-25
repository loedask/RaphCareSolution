using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Communication.DTOs;
using RaphCare.Domain.Communication;

namespace RaphCare.Application.Features.Communication.Queries.GetMessageById;

public class GetMessageByIdHandler : IRequestHandler<GetMessageByIdQuery, MessageDto>
{
    private readonly IRepository<Message> _repository;

    public GetMessageByIdHandler(IRepository<Message> repository)
    {
        _repository = repository;
    }

    public async Task<MessageDto> Handle(GetMessageByIdQuery request, CancellationToken cancellationToken)
    {
        var message = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (message is null)
        {
            throw new NotFoundException(nameof(Message), request.Id);
        }

        return new MessageDto
        {
            Id = message.Id,
            ClinicId = message.ClinicId,
            RecipientUserId = message.RecipientUserId,
            Channel = message.Channel,
            Subject = message.Subject,
            Body = message.Body,
            SentAt = message.SentAt,
            IsSent = message.IsSent
        };
    }
}
