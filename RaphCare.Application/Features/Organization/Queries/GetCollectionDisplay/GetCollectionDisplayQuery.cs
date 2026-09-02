using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetCollectionDisplay;

/// <summary>Public waiting-screen board (codes only). No sign-in.</summary>
public sealed class GetCollectionDisplayQuery : IRequest<CollectionDisplayBoardDto?>, IAllowAnonymousRequest
{
    public string Token { get; set; } = string.Empty;
}
