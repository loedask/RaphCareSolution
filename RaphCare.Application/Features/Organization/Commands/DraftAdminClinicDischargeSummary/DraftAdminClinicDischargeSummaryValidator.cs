using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.DraftAdminClinicDischargeSummary;

public sealed class DraftAdminClinicDischargeSummaryValidator : AbstractValidator<DraftAdminClinicDischargeSummaryCommand>
{
    public DraftAdminClinicDischargeSummaryValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.AdmissionId).NotEmpty();
    }
}
