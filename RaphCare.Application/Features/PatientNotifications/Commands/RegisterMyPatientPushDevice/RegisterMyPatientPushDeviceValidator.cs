using FluentValidation;

namespace RaphCare.Application.Features.PatientNotifications.Commands.RegisterMyPatientPushDevice;

public sealed class RegisterMyPatientPushDeviceValidator : AbstractValidator<RegisterMyPatientPushDeviceCommand>
{
    private static readonly string[] AllowedPlatforms = ["android", "ios", "web", "unknown"];

    public RegisterMyPatientPushDeviceValidator()
    {
        RuleFor(x => x.DeviceToken).NotEmpty().MinimumLength(8).MaximumLength(512);
        RuleFor(x => x.Platform).NotEmpty().MaximumLength(32)
            .Must(p => AllowedPlatforms.Contains(p.Trim().ToLowerInvariant()))
            .WithMessage($"Platform must be one of: {string.Join(", ", AllowedPlatforms)}.");
    }
}
