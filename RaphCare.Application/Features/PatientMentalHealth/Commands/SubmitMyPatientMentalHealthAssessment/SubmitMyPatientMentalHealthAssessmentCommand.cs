using MediatR;
using RaphCare.Application.Features.MentalHealth.DTOs;

namespace RaphCare.Application.Features.PatientMentalHealth.Commands.SubmitMyPatientMentalHealthAssessment;

public sealed class SubmitMyPatientMentalHealthAssessmentCommand : IRequest<MentalHealthAssessmentDetailDto?>
{
    public Guid ClinicId { get; set; }
    public string AssessmentType { get; set; } = "PHQ-9";
    public IReadOnlyList<SubmitMyPatientMentalHealthAssessmentAnswer> Answers { get; set; } =
        Array.Empty<SubmitMyPatientMentalHealthAssessmentAnswer>();
}

public sealed class SubmitMyPatientMentalHealthAssessmentAnswer
{
    public int Order { get; set; }
    public int NumericScore { get; set; }
}
