using MediatR;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.PatientBilling.Commands.CompleteCarePlanCheckoutFromPaystack;

/// <summary>Webhook-driven activation after Paystack reports a successful charge.</summary>
public sealed class CompleteCarePlanCheckoutFromPaystackCommand : IRequest<Unit>, IAllowAnonymousRequest
{
    public string Reference { get; set; } = string.Empty;
}
