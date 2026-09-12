using MediatR;

namespace RaphCare.Application.Features.Ops.Queries.GetClinicCommercialPlan;

public sealed class GetClinicCommercialPlanQuery : IRequest<ClinicCommercialPlanDto?>
{
    public Guid ClinicId { get; init; }
}

public sealed class ClinicCommercialPlanDto
{
    public Guid ClinicId { get; init; }
    public string CommercialPlan { get; init; } = string.Empty;
    public bool HasInpatient { get; init; }
    public bool HasCollection { get; init; }
    public bool HasCasualty { get; init; }
    public bool HasTheatre { get; init; }
    public bool HasConsultWaiting { get; init; }
}
