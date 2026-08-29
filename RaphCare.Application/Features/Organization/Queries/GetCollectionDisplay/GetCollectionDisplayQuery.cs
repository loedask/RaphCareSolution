using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetCollectionDisplay;

public sealed class GetCollectionDisplayQuery : IRequest<CollectionDisplayBoardDto?>
{
    public string Token { get; set; } = string.Empty;
}
