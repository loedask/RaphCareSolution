using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Insurance.DTOs;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Insurance.Queries.GetInsuranceProfiles;

public class GetInsuranceProfilesHandler : IRequestHandler<GetInsuranceProfilesQuery, PagedResult<InsuranceProfileDto>>
{
    private readonly IRepository<InsuranceProfile> _repository;

    public GetInsuranceProfilesHandler(IRepository<InsuranceProfile> repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<InsuranceProfileDto>> Handle(GetInsuranceProfilesQuery request, CancellationToken cancellationToken)
    {
        var profiles = await _repository.ListAsync(cancellationToken);

        var totalCount = profiles.Count;

        var items = profiles
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new InsuranceProfileDto
            {
                Id = p.Id,
                PatientId = p.PatientId,
                InsurancePlanId = p.InsurancePlanId,
                MembershipNumber = p.MembershipNumber,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsActive = p.IsActive
            })
            .ToList();

        return new PagedResult<InsuranceProfileDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

