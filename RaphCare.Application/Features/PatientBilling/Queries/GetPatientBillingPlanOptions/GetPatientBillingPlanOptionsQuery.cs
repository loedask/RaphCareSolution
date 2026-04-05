using MediatR;
using RaphCare.Application.Features.PatientBilling.DTOs;

namespace RaphCare.Application.Features.PatientBilling.Queries.GetPatientBillingPlanOptions;

public sealed class GetPatientBillingPlanOptionsQuery : IRequest<IReadOnlyList<PatientBillingPlanOptionDto>>;
