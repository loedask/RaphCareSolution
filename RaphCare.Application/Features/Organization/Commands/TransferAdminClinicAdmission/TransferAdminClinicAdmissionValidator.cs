using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.TransferAdminClinicAdmission;

public sealed class TransferAdminClinicAdmissionValidator : AbstractValidator<TransferAdminClinicAdmissionCommand>
{
    public TransferAdminClinicAdmissionValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.AdmissionId).NotEmpty();
        RuleFor(x => x.TargetBedId).NotEmpty();
        RuleFor(x => x.Notes).MaximumLength(1000).When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
