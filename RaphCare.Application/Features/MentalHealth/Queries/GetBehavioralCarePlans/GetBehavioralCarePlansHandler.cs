using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.MentalHealth.DTOs;
using RaphCare.Application.Features.Organization;
using RaphCare.Domain.MentalHealth;

namespace RaphCare.Application.Features.MentalHealth.Queries.GetBehavioralCarePlans;

public sealed class GetBehavioralCarePlansHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<BehavioralCarePlan> plans)
    : IRequestHandler<GetBehavioralCarePlansQuery, PagedResult<BehavioralCarePlanDto>>
{
    public async Task<PagedResult<BehavioralCarePlanDto>> Handle(
        GetBehavioralCarePlansQuery request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can view behavioral care plans.",
                cancellationToken)
            .ConfigureAwait(false);

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;
        var patientId = request.PatientId;

        var page = await plans.SearchAsync(
            q =>
            {
                q = q.Where(p => p.ClinicId == request.ClinicId).Include(p => p.Goals);
                if (patientId is Guid pid)
                    q = q.Where(p => p.PatientId == pid);
                return q.OrderByDescending(p => p.StartDate);
            },
            pageNumber,
            pageSize,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        return new PagedResult<BehavioralCarePlanDto>
        {
            Items = page.Items.Select(TherapyMapper.ToCarePlanDto).ToList(),
            TotalCount = page.TotalCount,
            PageNumber = page.PageNumber,
            PageSize = page.PageSize
        };
    }
}
