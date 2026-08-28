using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Api;
using RaphCare.Client.Models.Appointments;
using RaphCare.Client.Models.MentalHealth;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public sealed class PatientMentalHealthService(HttpClient httpClient) : BaseHttpService(httpClient), IPatientMentalHealthService
{
    public async Task<Response<PatientMentalHealthContentViewModel>> GetContentAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<MentalHealthContentDto>("api/patient/mental-health/content", cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<PatientMentalHealthContentViewModel>.Failure(result.ErrorMessage ?? "Could not load content.", result.StatusCode);
        return Response<PatientMentalHealthContentViewModel>.Success(Map(result.Data));
    }

    public async Task<Response<Guid>> LogMoodCheckInAsync(int moodScore, string? notes, CancellationToken cancellationToken = default)
    {
        var result = await PostAsync<CreatedGuidApiResponse>(
                "api/patient/mental-health/mood-checkin",
                new { moodScore, notes },
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.IsSuccess || result.Data is null)
            return Response<Guid>.Failure(result.ErrorMessage ?? "Check-in failed.", result.StatusCode);
        return Response<Guid>.Success(result.Data.Id);
    }

    private static PatientMentalHealthContentViewModel Map(MentalHealthContentDto d) =>
        new()
        {
            InsightTitle = d.InsightTitle ?? string.Empty,
            InsightBody = d.InsightBody ?? string.Empty,
            MedicalDisclaimer = d.MedicalDisclaimer ?? string.Empty
        };

    private sealed class MentalHealthContentDto
    {
        public string? InsightTitle { get; set; }
        public string? InsightBody { get; set; }
        public string? MedicalDisclaimer { get; set; }
    }
}
