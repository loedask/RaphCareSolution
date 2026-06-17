using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Communication.DTOs;
using RaphCare.Domain.Communication;

namespace RaphCare.Application.Features.Communication.Queries.GetMessages;

public class GetMessagesHandler : IRequestHandler<GetMessagesQuery, PagedResult<MessageDto>>
{
    private readonly IRepository<Message> _repository;

    public GetMessagesHandler(IRepository<Message> repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<MessageDto>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        var messages = await _repository.ListAsync(cancellationToken);

        var totalCount = messages.Count;

        var items = messages
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                ClinicId = m.ClinicId,
                RecipientUserId = m.RecipientUserId,
                Channel = m.Channel,
                Subject = m.Subject,
                Body = m.Body,
                SentAt = m.SentAt,
                IsSent = m.IsSent
            })
            .ToList();

        return new PagedResult<MessageDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
