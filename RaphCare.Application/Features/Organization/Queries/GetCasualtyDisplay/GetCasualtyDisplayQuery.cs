using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetCasualtyDisplay;

public sealed class GetCasualtyDisplayQuery : IRequest<CasualtyDisplayBoardDto?>
{
    public string Token { get; set; } = string.Empty;
}
