using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientBilling.DTOs;
using RaphCare.Domain.Billing;

namespace RaphCare.Application.Features.PatientBilling.Queries.GetPatientBillingPlanOptions;

public sealed class GetPatientBillingPlanOptionsHandler(IRepository<PriceCatalogItem> catalogRepository)
    : IRequestHandler<GetPatientBillingPlanOptionsQuery, IReadOnlyList<PatientBillingPlanOptionDto>>
{
    public async Task<IReadOnlyList<PatientBillingPlanOptionDto>> Handle(
        GetPatientBillingPlanOptionsQuery request,
        CancellationToken cancellationToken)
    {
        var items = await catalogRepository.ListAsync(cancellationToken).ConfigureAwait(false);
        return PatientBillingCatalog.FromCatalog(items);
    }
}
