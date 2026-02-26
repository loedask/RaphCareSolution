using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.MentalHealth.DTOs;

namespace RaphCare.Application.Features.MentalHealth.Queries.GetMentalHealthAssessments;

/// <summary>Use case: get a paginated list of mental health assessments for a clinic.</summary>
public class GetMentalHealthAssessmentsQuery : IRequest<PagedResult<MentalHealthAssessmentDto>>
{
    public Guid ClinicId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
