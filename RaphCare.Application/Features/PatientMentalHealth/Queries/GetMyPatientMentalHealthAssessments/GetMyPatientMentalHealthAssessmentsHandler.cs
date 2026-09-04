using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.MentalHealth;
using RaphCare.Application.Features.MentalHealth.DTOs;
using RaphCare.Domain.MentalHealth;

namespace RaphCare.Application.Features.PatientMentalHealth.Queries.GetMyPatientMentalHealthAssessments;

public sealed class GetMyPatientMentalHealthAssessmentsHandler(
    ICurrentUserService currentUser,
    IRepository<MentalHealthAssessment> assessments)
    : IRequestHandler<GetMyPatientMentalHealthAssessmentsQuery, PagedResult<MentalHealthAssessmentDto>>
{
    public async Task<PagedResult<MentalHealthAssessmentDto>> Handle(
        GetMyPatientMentalHealthAssessmentsQuery request,
        CancellationToken cancellationToken)
    {
        var patientId = currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;
        var clinicId = request.ClinicId;

        var page = await assessments.SearchAsync(
            q =>
            {
                q = q.Where(a => a.PatientId == patientId);
                if (clinicId is Guid cid)
                    q = q.Where(a => a.ClinicId == cid);
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
