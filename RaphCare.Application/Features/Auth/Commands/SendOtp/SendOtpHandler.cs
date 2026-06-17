using MediatR;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Auth.Commands.SendOtp;

public class SendOtpHandler : IRequestHandler<SendOtpCommand>
{
    private readonly IOtpService _otpService;
    private readonly ISmsService _smsService;

    public SendOtpHandler(IOtpService otpService, ISmsService smsService)
    {
        _otpService = otpService;
        _smsService = smsService;
    }

    public async Task Handle(SendOtpCommand request, CancellationToken cancellationToken)
    {
        var code = await _otpService.GenerateOtpAsync(request.PhoneNumber, cancellationToken);
        var message = $"Your RaphCare verification code is: {code}";
        await _smsService.SendSmsAsync(request.PhoneNumber, message, cancellationToken);
    }
}

