using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.PatientInsurance.DTOs;

namespace RaphCare.Application.Features.PatientInsurance.Queries.GetMyInsuranceProfiles;

public class GetMyInsuranceProfilesQuery : IRequest<PagedResult<PatientInsuranceProfileDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
