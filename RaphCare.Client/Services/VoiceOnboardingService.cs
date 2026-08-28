using System.Net.Http.Headers;
using System.Net.Http.Json;
using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Onboarding;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public sealed class VoiceOnboardingService(IHttpClientFactory httpClientFactory) : IVoiceOnboardingService
{
    public async Task<Response<VoiceOnboardingResultViewModel>> SubmitVoiceAsync(
        Stream audioStream,
        string fileName,
        string language,
        string phoneNumber,
        Guid clinicId,
        CancellationToken cancellationToken = default)
    {
        if (audioStream is null)
            return Response<VoiceOnboardingResultViewModel>.Failure("Audio file is required.");
        if (string.IsNullOrWhiteSpace(language))
            return Response<VoiceOnboardingResultViewModel>.Failure("Language is required.");
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return Response<VoiceOnboardingResultViewModel>.Failure("Phone number is required.");
        if (clinicId == Guid.Empty)
            return Response<VoiceOnboardingResultViewModel>.Failure("Clinic is required.");

        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var content = new MultipartFormDataContent();
            var audioContent = new StreamContent(audioStream);
            audioContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
            content.Add(audioContent, "audioFile", string.IsNullOrWhiteSpace(fileName) ? "recording.wav" : fileName.Trim());
            content.Add(new StringContent(language.Trim()), "language");
            content.Add(new StringContent(phoneNumber.Trim()), "phoneNumber");
            content.Add(new StringContent(clinicId.ToString()), "clinicId");

            using var response = await client.PostAsync("api/onboarding/voice", content, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                return Response<VoiceOnboardingResultViewModel>.Failure(
                    string.IsNullOrEmpty(err) ? $"API error: {response.StatusCode}" : err,
                    (int)response.StatusCode);
            }

            var result = await response.Content.ReadFromJsonAsync<VoiceResultDto>(ApiJson.Options, cancellationToken).ConfigureAwait(false);
            if (result is null)
                return Response<VoiceOnboardingResultViewModel>.Failure("Empty response from server.");

            return Response<VoiceOnboardingResultViewModel>.Success(new VoiceOnboardingResultViewModel
            {
                PatientId = result.PatientId,
                Transcription = result.Transcription ?? string.Empty
            });
        }
        catch (HttpRequestException)
        {
            return Response<VoiceOnboardingResultViewModel>.Failure(
                "We couldn't reach the server. Check your connection and try again.");
        }
    }

    private sealed class VoiceResultDto
    {
        public Guid PatientId { get; set; }
        public string? Transcription { get; set; }
    }
}
