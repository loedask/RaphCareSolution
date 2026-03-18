using FluentValidation;

namespace RaphCare.Application.Features.Patients.Commands.CreatePatient;

/// <summary>
/// Validates incoming data for the create patient use case.
/// </summary>
public class CreatePatientValidator : AbstractValidator<CreatePatientCommand>
{
    /// <summary>
    /// Initializes the validation rules for <see cref="CreatePatientCommand"/>.
    /// </summary>
    public CreatePatientValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DateOfBirth).LessThan(DateTime.UtcNow);
    }
}

