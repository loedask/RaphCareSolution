using MediatR;
using RaphCare.Application.Features.MentalHealth.DTOs;

namespace RaphCare.Application.Features.MentalHealth.Queries.GetMentalHealthAssessmentById;

public sealed class GetMentalHealthAssessmentByIdQuery : IRequest<MentalHealthAssessmentDetailDto?>
{
    public Guid ClinicId { get; set; }
    public Guid AssessmentId { get; set; }
}
