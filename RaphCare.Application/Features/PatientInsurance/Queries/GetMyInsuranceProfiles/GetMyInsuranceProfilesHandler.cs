using System.Linq;
using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientInsurance.DTOs;
using RaphCare.Domain.Insurance;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.PatientInsurance.Queries.GetMyInsuranceProfiles;

public class GetMyInsuranceProfilesHandler : IRequestHandler<GetMyInsuranceProfilesQuery, PagedResult<PatientInsuranceProfileDto>>
{
    private readonly IRepository<InsuranceProfile> _profileRepository;
    private readonly IRepository<InsurancePlan> _planRepository;
    private readonly ICurrentUserService _currentUser;

    public GetMyInsuranceProfilesHandler(
        IRepository<InsuranceProfile> profileRepository,
        IRepository<InsurancePlan> planRepository,
        ICurrentUserService currentUser)
    {
        _profileRepository = profileRepository;
        _planRepository = planRepository;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<PatientInsuranceProfileDto>> Handle(GetMyInsuranceProfilesQuery request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required to view insurance.");

        var paged = await _profileRepository.SearchAsync(
            q => q.Where(p => p.PatientId == patientId).OrderByDescending(p => p.StartDate),
            request.PageNumber,
            request.PageSize,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var planIds = paged.Items.Select(p => p.InsurancePlanId).Distinct().ToList();
        var planById = await ResolvePlansAsync(planIds, cancellationToken).ConfigureAwait(false);

        var items = paged.Items.Select(p =>
        {
            planById.TryGetValue(p.InsurancePlanId, out var plan);
            return new PatientInsuranceProfileDto
            {
                Id = p.Id,
                InsurancePlanId = p.InsurancePlanId,
                PlanName = plan?.Name ?? string.Empty,
                PlanCode = plan?.Code ?? string.Empty,
                MembershipNumber = p.MembershipNumber,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsActive = p.IsActive
            };
        }).ToList();

        return new PagedResult<PatientInsuranceProfileDto>
        {
            Items = items,
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize
        };
    }

    private async Task<Dictionary<Guid, InsurancePlan>> ResolvePlansAsync(List<Guid> planIds, CancellationToken cancellationToken)
    {
        if (planIds.Count == 0)
            return new Dictionary<Guid, InsurancePlan>();

        var plans = await _planRepository.SearchAsync(
            q => q.Where(x => planIds.Contains(x.Id)),
            1,
            Math.Max(planIds.Count, 1),
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        return plans.Items.ToDictionary(x => x.Id);
    }
}
