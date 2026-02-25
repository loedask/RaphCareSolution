using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.Communication.DTOs;

namespace RaphCare.Application.Features.Communication.Queries.GetMessages;

public class GetMessagesQuery : IRequest<PagedResult<MessageDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
