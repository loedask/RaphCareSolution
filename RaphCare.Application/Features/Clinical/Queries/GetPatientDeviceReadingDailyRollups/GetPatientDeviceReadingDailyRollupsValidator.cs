using FluentValidation;

namespace RaphCare.Application.Features.Clinical.Queries.GetPatientDeviceReadingDailyRollups;

public sealed class GetPatientDeviceReadingDailyRollupsValidator : AbstractValidator<GetPatientDeviceReadingDailyRollupsQuery>
{
    public const int MaxRangeDays = 366;

    public GetPatientDeviceReadingDailyRollupsValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.ToUtc).GreaterThanOrEqualTo(x => x.FromUtc);
        RuleFor(x => x)
            .Must(x => (x.ToUtc - x.FromUtc).TotalDays <= MaxRangeDays)
            .WithMessage($"Date range must not exceed {MaxRangeDays} days.");
    }
}
