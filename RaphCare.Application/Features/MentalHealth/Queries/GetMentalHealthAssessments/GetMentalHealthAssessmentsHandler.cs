using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.MentalHealth.DTOs;
using RaphCare.Application.Features.Organization;
using RaphCare.Domain.MentalHealth;

namespace RaphCare.Application.Features.MentalHealth.Queries.GetMentalHealthAssessments;

public sealed class GetMentalHealthAssessmentsHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<MentalHealthAssessment> assessments)
    : IRequestHandler<GetMentalHealthAssessmentsQuery, PagedResult<MentalHealthAssessmentDto>>
{
    public async Task<PagedResult<MentalHealthAssessmentDto>> Handle(
        GetMentalHealthAssessmentsQuery request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can view mental health assessments.",
                cancellationToken)
            .ConfigureAwait(false);

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;
        var patientId = request.PatientId;

        var page = await assessments.SearchAsync(
            q =>
            {
                q = q.Where(a => a.ClinicId == request.ClinicId);
                if (patientId is Guid pid)
                    q = q.Where(a => a.PatientId == pid);
                return q.OrderByDescending(a => a.ConductedAt);
            },
            pageNumber,
            pageSize,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        return new PagedResult<MentalHealthAssessmentDto>
        {
            Items = page.Items.Select(MentalHealthAssessmentMapper.ToListDto).ToList(),
            TotalCount = page.TotalCount,
            PageNumber = page.PageNumber,
            PageSize = page.PageSize
        };
    }
}
