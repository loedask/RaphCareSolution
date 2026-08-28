using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicVisitPrescription;

public sealed class CreateAdminClinicVisitPrescriptionValidator : AbstractValidator<CreateAdminClinicVisitPrescriptionCommand>
{
    public CreateAdminClinicVisitPrescriptionValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.VisitId).NotEmpty();
        RuleFor(x => x)
            .Must(HasMedicationLine)
            .WithMessage("Add at least one medication.");
        RuleFor(x => x.Dosage).MaximumLength(256).When(x => !string.IsNullOrWhiteSpace(x.Dosage));
        RuleFor(x => x.Frequency).MaximumLength(256).When(x => !string.IsNullOrWhiteSpace(x.Frequency));
        RuleFor(x => x.DurationDays).GreaterThanOrEqualTo(0).LessThanOrEqualTo(3650);
        RuleFor(x => x.Notes).MaximumLength(256).When(x => !string.IsNullOrWhiteSpace(x.Notes));
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.MedicationName).NotEmpty().MaximumLength(256);
            item.RuleFor(i => i.Dosage).MaximumLength(256).When(i => !string.IsNullOrWhiteSpace(i.Dosage));
            item.RuleFor(i => i.Frequency).MaximumLength(256).When(i => !string.IsNullOrWhiteSpace(i.Frequency));
            item.RuleFor(i => i.DurationDays).GreaterThanOrEqualTo(0).LessThanOrEqualTo(3650);
        }).When(x => x.Items.Count > 0);
        RuleFor(x => x.MedicationName).MaximumLength(256).When(x => !string.IsNullOrWhiteSpace(x.MedicationName));
    }

    private static bool HasMedicationLine(CreateAdminClinicVisitPrescriptionCommand command) =>
        command.Items.Any(i => !string.IsNullOrWhiteSpace(i.MedicationName))
        || !string.IsNullOrWhiteSpace(command.MedicationName);
}
