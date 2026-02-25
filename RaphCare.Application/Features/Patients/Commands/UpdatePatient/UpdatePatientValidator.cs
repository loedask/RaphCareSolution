using FluentValidation;

namespace RaphCare.Application.Features.Patients.Commands.UpdatePatient;

public class UpdatePatientValidator : AbstractValidator<UpdatePatientCommand>
{
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

