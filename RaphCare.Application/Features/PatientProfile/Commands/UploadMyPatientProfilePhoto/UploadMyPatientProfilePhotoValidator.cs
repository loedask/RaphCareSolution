using FluentValidation;

namespace RaphCare.Application.Features.PatientProfile.Commands.UploadMyPatientProfilePhoto;

public sealed class UploadMyPatientProfilePhotoValidator : AbstractValidator<UploadMyPatientProfilePhotoCommand>
{
    public UploadMyPatientProfilePhotoValidator()
    {
        RuleFor(x => x.Content).NotNull();
        RuleFor(x => x.ContentType).NotEmpty();
    }
}
