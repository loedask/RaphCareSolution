using MediatR;
using RaphCare.Application.Features.Ops.Queries.GetClinicCommercialPlan;

namespace RaphCare.Application.Features.Ops.Commands.UpdateClinicCommercialPlan;

public sealed class UpdateClinicCommercialPlanCommand : IRequest<ClinicCommercialPlanDto?>
{
    public Guid ClinicId { get; init; }
    public string CommercialPlan { get; init; } = string.Empty;
}
