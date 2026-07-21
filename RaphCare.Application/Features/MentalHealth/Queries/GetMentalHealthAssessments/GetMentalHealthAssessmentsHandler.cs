using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.MentalHealth.DTOs;
using RaphCare.Domain.MentalHealth;

namespace RaphCare.Application.Features.MentalHealth.Queries.GetMentalHealthAssessments;

public sealed class GetMentalHealthAssessmentsHandler(IRepository<MentalHealthAssessment> assessments)
    : IRequestHandler<GetMentalHealthAssessmentsQuery, PagedResult<MentalHealthAssessmentDto>>
{
    public async Task<PagedResult<MentalHealthAssessmentDto>> Handle(
        GetMentalHealthAssessmentsQuery request,
        CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var page = await assessments.SearchAsync(
            q => q
                .Where(a => a.ClinicId == request.ClinicId)
                .OrderByDescending(a => a.ConductedAt),
            pageNumber,
            pageSize,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        return new PagedResult<MentalHealthAssessmentDto>
        {
            Items = page.Items.Select(a => new MentalHealthAssessmentDto
            {
                Id = a.Id,
                PatientId = a.PatientId,
                AssessedAt = a.ConductedAt,
                Summary = BuildSummary(a)
            }).ToList(),
            TotalCount = page.TotalCount,
            PageNumber = page.PageNumber,
            PageSize = page.PageSize
        };
    }

    private static string BuildSummary(MentalHealthAssessment a)
    {
        var score = a.TotalScore is null ? a.SeverityLevel : $"{a.SeverityLevel} (score {a.TotalScore})";
        return $"{a.AssessmentType}: {score}";
    }
}
