using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.MentalHealth.DTOs;

namespace RaphCare.Application.Features.MentalHealth.Queries.GetMentalHealthAssessments;

public class GetMentalHealthAssessmentsQuery : IRequest<PagedResult<MentalHealthAssessmentDto>>
{
    public Guid ClinicId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
