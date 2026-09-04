using MediatR;
using RaphCare.Application.Features.MentalHealth.DTOs;

namespace RaphCare.Application.Features.MentalHealth.Commands.ResolveCrisisFlag;

public sealed class ResolveCrisisFlagCommand : IRequest<CrisisFlagDto?>
{
    public Guid ClinicId { get; set; }
    public Guid SessionId { get; set; }
    public Guid CrisisFlagId { get; set; }
}
