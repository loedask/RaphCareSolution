using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.DeleteAdminClinicRosterEntry;

public sealed class DeleteAdminClinicRosterEntryValidator : AbstractValidator<DeleteAdminClinicRosterEntryCommand>
{
    public DeleteAdminClinicRosterEntryValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.EntryId).NotEmpty();
    }
}
