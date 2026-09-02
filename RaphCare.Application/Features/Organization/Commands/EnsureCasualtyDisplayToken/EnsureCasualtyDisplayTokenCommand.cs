using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.EnsureCasualtyDisplayToken;

public sealed class EnsureCasualtyDisplayTokenCommand : IRequest<CasualtyDisplayLinkDto?>
{
    public Guid ClinicId { get; set; }
}
