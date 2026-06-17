using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.MentalHealth;

public class AssessmentQuestion : BaseEntity
{
    public Guid MentalHealthAssessmentId { get; set; }

    public string QuestionText { get; set; } = null!;
    public int Order { get; set; }

    public MentalHealthAssessment Assessment { get; set; } = null!;
    public ICollection<AssessmentResponse> Responses { get; set; } = new List<AssessmentResponse>();
}

