using MediatR;

namespace RaphCare.Application.Features.PatientProfile.Commands.UploadMyPatientProfilePhoto;

public sealed class UploadMyPatientProfilePhotoCommand : IRequest<string>
{
    public required Stream Content { get; init; }
    public required string ContentType { get; init; }
}
