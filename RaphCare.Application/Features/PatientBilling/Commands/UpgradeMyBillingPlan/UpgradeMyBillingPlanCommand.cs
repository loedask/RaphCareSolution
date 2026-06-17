using MediatR;

namespace RaphCare.Application.Features.PatientBilling.Commands.UpgradeMyBillingPlan;

public sealed class UpgradeMyBillingPlanCommand : IRequest<Unit>
{
    public string PlanCode { get; set; } = string.Empty;
}
