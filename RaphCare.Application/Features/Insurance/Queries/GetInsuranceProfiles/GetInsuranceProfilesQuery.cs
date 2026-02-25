using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.Insurance.DTOs;

namespace RaphCare.Application.Features.Insurance.Queries.GetInsuranceProfiles;

public class GetInsuranceProfilesQuery : IRequest<PagedResult<InsuranceProfileDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

