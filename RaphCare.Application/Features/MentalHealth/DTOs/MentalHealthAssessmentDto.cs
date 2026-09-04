namespace RaphCare.Application.Features.MentalHealth.DTOs;

/// <summary>List or summary row for a staff mental health assessment.</summary>
public class MentalHealthAssessmentDto
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public string AssessmentType { get; set; } = string.Empty;
    public DateTime AssessedAt { get; set; }
    public decimal? TotalScore { get; set; }
    public string SeverityLevel { get; set; } = string.Empty;
    public string? Summary { get; set; }
}

/// <summary>Assessment with item-level answers.</summary>
public sealed class MentalHealthAssessmentDetailDto : MentalHealthAssessmentDto
{
    public IReadOnlyList<MentalHealthAssessmentItemDto> Items { get; set; } = Array.Empty<MentalHealthAssessmentItemDto>();
}

public sealed class MentalHealthAssessmentItemDto
{
    public int Order { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int? NumericScore { get; set; }
    public string ResponseValue { get; set; } = string.Empty;
}

/// <summary>Instrument definition used to render the staff recording form.</summary>
public sealed class MentalHealthInstrumentDto
{
    public string AssessmentType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public IReadOnlyList<MentalHealthInstrumentQuestionDto> Questions { get; set; } =
        Array.Empty<MentalHealthInstrumentQuestionDto>();
    public IReadOnlyList<MentalHealthInstrumentOptionDto> Options { get; set; } =
        Array.Empty<MentalHealthInstrumentOptionDto>();
}

public sealed class MentalHealthInstrumentQuestionDto
{
    public int Order { get; set; }
    public string QuestionText { get; set; } = string.Empty;
}

public sealed class MentalHealthInstrumentOptionDto
{
    public int NumericScore { get; set; }
    public string Label { get; set; } = string.Empty;
}
