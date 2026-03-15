using MediatR;

namespace RaphCare.Application.Features.Auth.Commands.VerifyOtp;

public class VerifyOtpCommand : IRequest<bool>
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

