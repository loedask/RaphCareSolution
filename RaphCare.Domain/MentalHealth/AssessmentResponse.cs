using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.MentalHealth;

public class AssessmentResponse : BaseEntity
{
    public Guid MentalHealthAssessmentId { get; set; }
    public Guid AssessmentQuestionId { get; set; }

    public string ResponseValue { get; set; } = null!;
    public int? NumericScore { get; set; }

    public MentalHealthAssessment Assessment { get; set; } = null!;
    public AssessmentQuestion Question { get; set; } = null!;
}

