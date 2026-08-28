using FluentValidation;

namespace RaphCare.Application.Features.PatientTelehealth.Commands.RequestOnDemandTelehealthSession;

public sealed class RequestOnDemandTelehealthSessionValidator : AbstractValidator<RequestOnDemandTelehealthSessionCommand>
{
    public RequestOnDemandTelehealthSessionValidator()
    {
        RuleFor(x => x.CallMode)
            .Must(m => m is null or "" or "video" or "audio" or "lowband")
            .WithMessage("CallMode must be video, audio, or lowband.");
    }
}
