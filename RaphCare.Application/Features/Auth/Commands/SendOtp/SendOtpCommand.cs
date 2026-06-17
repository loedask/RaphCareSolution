using MediatR;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Auth.Commands.SendOtp;

public class SendOtpCommand : IRequest, IAllowAnonymousRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
}

