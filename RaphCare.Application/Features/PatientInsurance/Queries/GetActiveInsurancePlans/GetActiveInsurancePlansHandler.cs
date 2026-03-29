using System.Linq;
using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientInsurance.DTOs;
using RaphCare.Domain.Insurance;

namespace RaphCare.Application.Features.PatientInsurance.Queries.GetActiveInsurancePlans;

public class GetActiveInsurancePlansHandler : IRequestHandler<GetActiveInsurancePlansQuery, IReadOnlyList<InsurancePlanOptionDto>>
{
    private readonly IRepository<InsurancePlan> _planRepository;

    public GetActiveInsurancePlansHandler(IRepository<InsurancePlan> planRepository)
    {
        _planRepository = planRepository;
    }

    public async Task<IReadOnlyList<InsurancePlanOptionDto>> Handle(GetActiveInsurancePlansQuery request, CancellationToken cancellationToken)
    {
        var paged = await _planRepository.SearchAsync(
            q => q.Where(p => p.IsActive).OrderBy(p => p.Name),
            1,
            500,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        return paged.Items
            .Select(p => new InsurancePlanOptionDto
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code
            })
            .ToList();
    }
}
