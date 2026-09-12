using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.UpsertAdminClinicConsentTemplate;

public sealed class UpsertAdminClinicConsentTemplateValidator : AbstractValidator<UpsertAdminClinicConsentTemplateCommand>
{
    public UpsertAdminClinicConsentTemplateValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Body).NotEmpty().MaximumLength(8000);
    }
}
