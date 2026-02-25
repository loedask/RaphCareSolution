using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.MentalHealth;

public class MentalHealthAssessment : BaseEntity, IAggregateRoot
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }

    public string AssessmentType { get; set; } = null!;
    public DateTime ConductedAt { get; set; }
    public decimal? TotalScore { get; set; }
    public string SeverityLevel { get; set; } = null!;
    public bool IsAIEnhanced { get; set; }

    public ICollection<AssessmentQuestion> Questions { get; set; } = new List<AssessmentQuestion>();
    public ICollection<AssessmentResponse> Responses { get; set; } = new List<AssessmentResponse>();
}

