using MediatR;
using RaphCare.Application.Features.PatientInsurance.DTOs;

namespace RaphCare.Application.Features.PatientInsurance.Queries.GetActiveInsurancePlans;

public class GetActiveInsurancePlansQuery : IRequest<IReadOnlyList<InsurancePlanOptionDto>>
{
}
