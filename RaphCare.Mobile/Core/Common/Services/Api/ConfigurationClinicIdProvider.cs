using Microsoft.Extensions.Options;
using RaphCare.Client.Contracts;
using RaphCare.Mobile.Core.Shared.Configuration;

namespace RaphCare.Mobile.Core.Shared.Services.Api;

public sealed class ConfigurationClinicIdProvider(IOptions<ApiMobileOptions> apiOptions, IOptions<OnboardingOptions> onboardingOptions)
    : IClinicIdProvider
{
    public Guid? GetClinicId()
    {
        var clinicId = apiOptions.Value.ClinicId;
        if (clinicId is { } id && id != Guid.Empty)
            return id;

        var voiceClinicId = onboardingOptions.Value.VoiceRegistrationClinicId;
        if (voiceClinicId is { } voiceId && voiceId != Guid.Empty)
            return voiceId;

        return null;
    }
}
