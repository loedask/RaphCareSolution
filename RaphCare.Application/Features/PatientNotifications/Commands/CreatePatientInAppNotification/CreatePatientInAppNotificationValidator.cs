using FluentValidation;

namespace RaphCare.Application.Features.PatientNotifications.Commands.CreatePatientInAppNotification;

public sealed class CreatePatientInAppNotificationValidator : AbstractValidator<CreatePatientInAppNotificationCommand>
{
    public CreatePatientInAppNotificationValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Body).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Type).NotEmpty().MaximumLength(64);
    }
}
