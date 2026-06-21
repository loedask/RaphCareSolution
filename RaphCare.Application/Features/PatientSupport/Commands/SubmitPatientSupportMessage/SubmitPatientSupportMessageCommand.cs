using MediatR;

namespace RaphCare.Application.Features.PatientSupport.Commands.SubmitPatientSupportMessage;

public sealed class SubmitPatientSupportMessageCommand : IRequest<Guid>
{
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
