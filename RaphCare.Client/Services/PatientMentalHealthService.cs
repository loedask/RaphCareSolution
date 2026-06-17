using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.MentalHealth;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

/// <summary>Wraps generated <see cref="IClient"/> patient mental health operations and maps to feature view models.</summary>
public sealed class PatientMentalHealthService(IClient client) : IPatientMentalHealthService
{
    public async Task<Response<PatientMentalHealthContentViewModel>> GetContentAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = await client.GetMyPatientMentalHealthContentAsync(cancellationToken).ConfigureAwait(false);
            return Response<PatientMentalHealthContentViewModel>.Success(Map(dto));
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<PatientMentalHealthContentViewModel>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<Guid>> LogMoodCheckInAsync(int moodScore, string? notes, CancellationToken cancellationToken = default)
    {
        try
        {
            var body = new LogMyPatientMoodCheckInCommand
            {
                MoodScore = moodScore,
                Notes = notes
            };
            var created = await client.LogMyPatientMoodCheckInAsync(body, cancellationToken).ConfigureAwait(false);
            return Response<Guid>.Success(created.Id);
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<Guid>.Failure(ex.Message, ex.StatusCode);
        }
    }

    private static PatientMentalHealthContentViewModel Map(PatientMentalHealthContentDto d) =>
        new()
        {
            InsightTitle = d.InsightTitle ?? string.Empty,
            InsightBody = d.InsightBody ?? string.Empty,
            MedicalDisclaimer = d.MedicalDisclaimer ?? string.Empty
        };
}
