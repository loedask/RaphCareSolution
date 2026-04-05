using MediatR;
using RaphCare.Application.Features.PatientBilling.DTOs;

namespace RaphCare.Application.Features.PatientBilling.Queries.GetPatientBillingPlanOptions;

public sealed class GetPatientBillingPlanOptionsHandler : IRequestHandler<GetPatientBillingPlanOptionsQuery, IReadOnlyList<PatientBillingPlanOptionDto>>
{
    public Task<IReadOnlyList<PatientBillingPlanOptionDto>> Handle(
        GetPatientBillingPlanOptionsQuery request,
        CancellationToken cancellationToken) =>
        Task.FromResult(PatientBillingCatalog.All);
}
