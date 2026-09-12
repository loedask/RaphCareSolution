using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetConsultDisplay;

/// <summary>Public consult waiting-screen board (codes only). No sign-in.</summary>
public sealed class GetConsultDisplayQuery : IRequest<ConsultDisplayBoardDto?>, IAllowAnonymousRequest
{
    public string Token { get; set; } = string.Empty;
}
