using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicVisitPrescription;

public sealed class CreateAdminClinicVisitPrescriptionValidator : AbstractValidator<CreateAdminClinicVisitPrescriptionCommand>
{
    public CreateAdminClinicVisitPrescriptionValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.VisitId).NotEmpty();
        RuleFor(x => x.MedicationName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Dosage).MaximumLength(256).When(x => !string.IsNullOrWhiteSpace(x.Dosage));
        RuleFor(x => x.Frequency).MaximumLength(256).When(x => !string.IsNullOrWhiteSpace(x.Frequency));
        RuleFor(x => x.DurationDays).GreaterThanOrEqualTo(0).LessThanOrEqualTo(3650);
        RuleFor(x => x.Notes).MaximumLength(256).When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
