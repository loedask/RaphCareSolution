using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetCasualtyDisplay;

/// <summary>Public casualty waiting-screen board (codes only). No sign-in.</summary>
public sealed class GetCasualtyDisplayQuery : IRequest<CasualtyDisplayBoardDto?>, IAllowAnonymousRequest
{
    public string Token { get; set; } = string.Empty;
}
