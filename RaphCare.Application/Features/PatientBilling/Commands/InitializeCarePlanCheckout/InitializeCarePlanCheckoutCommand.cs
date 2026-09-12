using MediatR;
using RaphCare.Application.Features.PatientBilling.DTOs;

namespace RaphCare.Application.Features.PatientBilling.Commands.InitializeCarePlanCheckout;

public sealed class InitializeCarePlanCheckoutCommand : IRequest<CarePlanCheckoutDto>
{
    public string PlanCode { get; set; } = string.Empty;
    public string? CallbackUrl { get; set; }
}
