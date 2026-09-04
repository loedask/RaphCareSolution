using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.MentalHealth.DTOs;

namespace RaphCare.Application.Features.PatientMentalHealth.Queries.GetMyPatientMentalHealthAssessments;

public sealed class GetMyPatientMentalHealthAssessmentsQuery : IRequest<PagedResult<MentalHealthAssessmentDto>>
{
    public Guid? ClinicId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
