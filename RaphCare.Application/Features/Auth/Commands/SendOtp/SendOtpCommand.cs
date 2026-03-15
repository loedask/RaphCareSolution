using MediatR;

namespace RaphCare.Application.Features.Auth.Commands.SendOtp;

public class SendOtpCommand : IRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
}

