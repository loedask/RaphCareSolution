using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.EnsureCollectionDisplayToken;

public sealed class EnsureCollectionDisplayTokenCommand : IRequest<CollectionDisplayLinkDto?>
{
    public Guid ClinicId { get; set; }
}
