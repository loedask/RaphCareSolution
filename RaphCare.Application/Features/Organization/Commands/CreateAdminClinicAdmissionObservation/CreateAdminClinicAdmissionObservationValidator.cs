using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicAdmissionObservation;

public sealed class CreateAdminClinicAdmissionObservationValidator
    : AbstractValidator<CreateAdminClinicAdmissionObservationCommand>
{
    public CreateAdminClinicAdmissionObservationValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.AdmissionId).NotEmpty();
        RuleFor(x => x.Note).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.HeartRate).GreaterThan(0).When(x => x.HeartRate.HasValue);
        RuleFor(x => x.TemperatureCelsius).InclusiveBetween(30, 45).When(x => x.TemperatureCelsius.HasValue);
        RuleFor(x => x.OxygenSaturation).InclusiveBetween(50, 100).When(x => x.OxygenSaturation.HasValue);
        RuleFor(x => x.SystolicBp).GreaterThan(0).When(x => x.SystolicBp.HasValue);
        RuleFor(x => x.DiastolicBp).GreaterThan(0).When(x => x.DiastolicBp.HasValue);
    }
}
