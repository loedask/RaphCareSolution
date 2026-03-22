using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Onboarding;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public sealed class VoiceOnboardingService(IClient client) : IVoiceOnboardingService
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
            var file = new FileParameter(audioStream, string.IsNullOrWhiteSpace(fileName) ? "recording.wav" : fileName.Trim(), "audio/wav");
            var result = await client.VoiceAsync(file, language.Trim(), phoneNumber.Trim(), clinicId, cancellationToken).ConfigureAwait(false);
            return Response<VoiceOnboardingResultViewModel>.Success(new VoiceOnboardingResultViewModel
            {
                PatientId = result.PatientId,
                Transcription = result.Transcription ?? string.Empty
            });
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<VoiceOnboardingResultViewModel>.Failure(ex.Message, ex.StatusCode);
        }
    }
}
