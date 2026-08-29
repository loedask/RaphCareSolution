using Microsoft.Extensions.Options;
using RaphCare.Client.Contracts;
using RaphCare.Mobile.Core.Common.Configuration;

namespace RaphCare.Mobile.Core.Common.Services.Api;

/// <summary>
/// Supplies <c>X-Clinic-Id</c>: user-selected clinic first, then Api / Onboarding / Appointments config.
/// </summary>
public sealed class MobileClinicIdProvider(
    ISelectedClinicStore selectedClinicStore,
    IOptions<ApiMobileOptions> apiOptions,
    IOptions<OnboardingOptions> onboardingOptions,
    IOptions<AppointmentsMobileOptions> appointmentsOptions)
    : IClinicIdProvider
{
    public Guid? GetClinicId()
    {
        Guid? appointmentsDefault = null;
        if (Guid.TryParse(appointmentsOptions.Value.DefaultClinicId, out var fromAppointments)
            && fromAppointments != Guid.Empty)
        {
            appointmentsDefault = fromAppointments;
        }

        return ClinicIdResolution.Resolve(
            selectedClinicStore.ClinicId,
            apiOptions.Value.ClinicId,
            onboardingOptions.Value.VoiceRegistrationClinicId,
            appointmentsDefault);
    }
}
