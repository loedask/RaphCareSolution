using FluentValidation;

namespace RaphCare.Application.Features.StandaloneEmergency.Commands.IngestDeviceEmergencyEvent;

public sealed class IngestDeviceEmergencyEventValidator : AbstractValidator<IngestDeviceEmergencyEventCommand>
{
    public IngestDeviceEmergencyEventValidator()
    {
        RuleFor(x => x.SerialNumber)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.EventType)
            .NotEmpty()
            .MaximumLength(64);
        RuleFor(x => x.ExternalEventId)
            .MaximumLength(256)
            .When(x => x.ExternalEventId != null);
        RuleFor(x => x.OccurredAtUtc)
            .Must(d => d.Kind is DateTimeKind.Utc or DateTimeKind.Unspecified)
            .WithMessage("occurredAtUtc must be UTC or unspecified (ISO-8601 with Z recommended).");
    }
}
