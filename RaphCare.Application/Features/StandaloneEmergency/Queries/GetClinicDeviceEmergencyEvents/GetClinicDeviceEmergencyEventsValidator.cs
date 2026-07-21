using FluentValidation;

namespace RaphCare.Application.Features.StandaloneEmergency.Queries.GetClinicDeviceEmergencyEvents;

public sealed class GetClinicDeviceEmergencyEventsValidator : AbstractValidator<GetClinicDeviceEmergencyEventsQuery>
{
    public GetClinicDeviceEmergencyEventsValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
