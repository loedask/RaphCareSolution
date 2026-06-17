using RaphCare.Client.Contracts;
using RaphCare.Client.Models.Onboarding;

namespace RaphCare.Client.Contracts.Interfaces;

public interface IVoiceOnboardingService
{
    Task<Response<VoiceOnboardingResultViewModel>> SubmitVoiceAsync(
        Stream audioStream,
        string fileName,
        string language,
        string phoneNumber,
        Guid clinicId,
        CancellationToken cancellationToken = default);
}
