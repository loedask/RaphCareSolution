using MediatR;
using RaphCare.Application.Features.MentalHealth.DTOs;

namespace RaphCare.Application.Features.MentalHealth.Commands.CreateMentalHealthAssessment;

public sealed class CreateMentalHealthAssessmentCommand : IRequest<MentalHealthAssessmentDetailDto?>
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public string AssessmentType { get; set; } = Phq9Instrument.AssessmentType;
    public IReadOnlyList<CreateMentalHealthAssessmentAnswer> Answers { get; set; } =
        Array.Empty<CreateMentalHealthAssessmentAnswer>();
}

public sealed class CreateMentalHealthAssessmentAnswer
{
    public int Order { get; set; }
    public int NumericScore { get; set; }
}
