using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.AdmitAdminClinicPatient;

public sealed class AdmitAdminClinicPatientValidator : AbstractValidator<AdmitAdminClinicPatientCommand>
{
    public AdmitAdminClinicPatientValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.BedId).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Reason));
        RuleFor(x => x.Notes).MaximumLength(1000).When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
