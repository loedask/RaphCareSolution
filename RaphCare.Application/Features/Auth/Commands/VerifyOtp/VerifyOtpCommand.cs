using MediatR;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Auth.Commands.VerifyOtp;

public class VerifyOtpResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
}

public class VerifyOtpCommand : IRequest<VerifyOtpResult>, IAllowAnonymousRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

