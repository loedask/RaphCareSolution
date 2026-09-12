using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.EnsureConsultDisplayToken;

public sealed class EnsureConsultDisplayTokenCommand : IRequest<ConsultDisplayLinkDto?>
{
    public Guid ClinicId { get; set; }
}
