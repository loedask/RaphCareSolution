using RaphCare.Domain.MentalHealth;

namespace RaphCare.Application.Features.MentalHealth;

/// <summary>Builds a scored assessment aggregate from instrument answers.</summary>
public static class MentalHealthAssessmentFactory
{
    public static MentalHealthAssessment Create(
        Guid clinicId,
        Guid patientId,
        MentalHealthInstruments.ResolvedInstrument instrument,
        IReadOnlyDictionary<int, int> answersByOrder,
        DateTime conductedAt)
    {
        var totalScore = answersByOrder.Values.Sum();
        var assessment = new MentalHealthAssessment
        {
            ClinicId = clinicId,
            PatientId = patientId,
            AssessmentType = instrument.AssessmentType,
            ConductedAt = conductedAt,
            TotalScore = totalScore,
            SeverityLevel = instrument.SeverityForTotal(totalScore),
            IsAIEnhanced = false
        };

        for (var i = 0; i < instrument.QuestionCount; i++)
        {
            var order = i + 1;
            var score = answersByOrder[order];
            var question = new AssessmentQuestion
            {
                MentalHealthAssessmentId = assessment.Id,
                QuestionText = instrument.QuestionTexts[i],
                Order = order
            };
            var response = new AssessmentResponse
            {
                MentalHealthAssessmentId = assessment.Id,
                AssessmentQuestionId = question.Id,
                NumericScore = score,
                ResponseValue = ScreeningInstrumentOptions.OptionLabel(score)
            };
            assessment.Questions.Add(question);
            assessment.Responses.Add(response);
        }

        return assessment;
    }
}
