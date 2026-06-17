using FluentValidation;

namespace RaphCare.Application.Features.Patients.Commands.UpdatePatient;

/// <summary>
/// Validates incoming data for the update patient use case.
/// </summary>
public class UpdatePatientValidator : AbstractValidator<UpdatePatientCommand>
{
    /// <summary>
    /// Initializes validation rules for <see cref="UpdatePatientCommand"/>.
    /// </summary>
    public UpdatePatientValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        When(x => x.FirstName != null, () =>
        {
            RuleFor(x => x.FirstName!)
                .NotEmpty()
                .MaximumLength(100);
        });

        When(x => x.LastName != null, () =>
        {
            RuleFor(x => x.LastName!)
                .NotEmpty()
                .MaximumLength(100);
        });
    }
}

