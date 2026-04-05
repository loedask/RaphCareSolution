using MediatR;

namespace RaphCare.Application.Features.PatientBilling.Commands.AddMyPaymentMethod;

public sealed class AddMyPaymentMethodCommand : IRequest<Guid>
{
    public string MethodType { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string MaskedDetails { get; set; } = string.Empty;
    public bool SetAsDefault { get; set; }
}
