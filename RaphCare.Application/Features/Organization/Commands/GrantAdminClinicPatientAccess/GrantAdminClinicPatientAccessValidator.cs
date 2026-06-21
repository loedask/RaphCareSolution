using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.GrantAdminClinicPatientAccess;

public sealed class GrantAdminClinicPatientAccessValidator : AbstractValidator<GrantAdminClinicPatientAccessCommand>
{
    public GrantAdminClinicPatientAccessValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Notes).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
