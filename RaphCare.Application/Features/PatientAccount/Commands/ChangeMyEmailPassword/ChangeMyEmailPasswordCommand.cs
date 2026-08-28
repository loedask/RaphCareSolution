using MediatR;

namespace RaphCare.Application.Features.PatientAccount.Commands.ChangeMyEmailPassword;

public sealed class ChangeMyEmailPasswordCommand : IRequest<Unit>
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
