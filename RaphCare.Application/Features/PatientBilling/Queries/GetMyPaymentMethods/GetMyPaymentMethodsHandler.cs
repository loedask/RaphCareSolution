using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientBilling.DTOs;
using RaphCare.Domain.Billing;

namespace RaphCare.Application.Features.PatientBilling.Queries.GetMyPaymentMethods;

public sealed class GetMyPaymentMethodsHandler(
    IRepository<PaymentMethod> methods,
    ICurrentUserService currentUser) : IRequestHandler<GetMyPaymentMethodsQuery, IReadOnlyList<PatientPaymentMethodDto>>
{
    private readonly IRepository<PaymentMethod> _methods = methods;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<IReadOnlyList<PatientPaymentMethodDto>> Handle(
        GetMyPaymentMethodsQuery request,
        CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var paged = await _methods.SearchAsync(
            q => q
                .Where(m => m.PatientId == patientId && m.IsActive)
                .OrderByDescending(m => m.IsDefault)
                .ThenBy(m => m.ProviderName),
            1,
            200,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        return paged.Items.Select(m => new PatientPaymentMethodDto
        {
            Id = m.Id,
            MethodType = m.MethodType,
            ProviderName = m.ProviderName,
            MaskedDetails = m.MaskedDetails,
            IsDefault = m.IsDefault
        }).ToList();
    }
}
