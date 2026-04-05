using FluentValidation;

namespace RaphCare.Application.Features.StandaloneEmergency.Queries.GetPatientDeviceEmergencyEvents;

public sealed class GetPatientDeviceEmergencyEventsValidator : AbstractValidator<GetPatientDeviceEmergencyEventsQuery>
{
    public GetPatientDeviceEmergencyEventsValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
    }
}
