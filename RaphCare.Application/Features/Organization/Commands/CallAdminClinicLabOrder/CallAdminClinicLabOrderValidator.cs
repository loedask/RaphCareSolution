using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.CallAdminClinicLabOrder;

public sealed class CallAdminClinicLabOrderValidator : AbstractValidator<CallAdminClinicLabOrderCommand>
{
    public CallAdminClinicLabOrderValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.LabRequestId).NotEmpty();
    }
}
