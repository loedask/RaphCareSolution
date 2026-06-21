using FluentValidation;

namespace RaphCare.Application.Features.PatientMedicalInfo.Commands.UpdateMyPatientMedicalInfo;

public sealed class UpdateMyPatientMedicalInfoValidator : AbstractValidator<UpdateMyPatientMedicalInfoCommand>
{
    private static readonly string[] AllowedBloodTypes =
        ["A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-", ""];

    public UpdateMyPatientMedicalInfoValidator()
    {
        RuleFor(x => x.BloodType)
            .Must(bt => AllowedBloodTypes.Contains(bt.Trim(), StringComparer.OrdinalIgnoreCase))
            .WithMessage("Blood type must be a standard ABO/Rh value or empty.");
        RuleFor(x => x.Allergies).MaximumLength(2000);
        RuleFor(x => x.ChronicConditions).MaximumLength(2000);
        RuleFor(x => x.Medications).MaximumLength(2000);
        RuleFor(x => x.PrimaryDoctor).MaximumLength(256);
    }
}
