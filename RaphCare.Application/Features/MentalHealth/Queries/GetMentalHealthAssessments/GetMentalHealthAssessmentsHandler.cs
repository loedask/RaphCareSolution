using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.MentalHealth.DTOs;

namespace RaphCare.Application.Features.MentalHealth.Queries.GetMentalHealthAssessments;

/// <summary>
/// Placeholder handler until mental health persistence is implemented.
/// </summary>
public class GetMentalHealthAssessmentsHandler : IRequestHandler<GetMentalHealthAssessmentsQuery, PagedResult<MentalHealthAssessmentDto>>
{
    public Task<PagedResult<MentalHealthAssessmentDto>> Handle(GetMentalHealthAssessmentsQuery request, CancellationToken cancellationToken)
    {
        var result = new PagedResult<MentalHealthAssessmentDto>
        {
            Items = Array.Empty<MentalHealthAssessmentDto>(),
            TotalCount = 0,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
        return Task.FromResult(result);
    }
}
