using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Options;
using RaphCare.Application.Common.Configuration;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Patients;
using RaphCare.Domain.Telemedicine;

namespace RaphCare.Application.Features.PatientTelehealth.Commands.SendTelehealthSessionSms;

public class SendTelehealthSessionSmsHandler : IRequestHandler<SendTelehealthSessionSmsCommand, Unit>
{
    private readonly IRepository<TeleSession> _teleSessions;
    private readonly IRepository<Patient> _patients;
    private readonly ICurrentUserService _currentUser;
    private readonly ISmsService _smsService;
    private readonly IOptionsSnapshot<TwilioSmsOptions> _twilioOptions;

    public SendTelehealthSessionSmsHandler(
        IRepository<TeleSession> teleSessions,
        IRepository<Patient> patients,
        ICurrentUserService currentUser,
        ISmsService smsService,
        IOptionsSnapshot<TwilioSmsOptions> twilioOptions)
    {
        _teleSessions = teleSessions;
        _patients = patients;
        _currentUser = currentUser;
        _smsService = smsService;
        _twilioOptions = twilioOptions;
    }

    public async Task<Unit> Handle(SendTelehealthSessionSmsCommand request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var twilio = _twilioOptions.Value;
        if (!twilio.IsEnabled)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(TwilioSmsOptions), "SMS is not configured. Set Twilio:AccountSid, Twilio:AuthToken, and Twilio:FromPhoneE164.")
            });
        }

        var session = await _teleSessions.GetByIdAsync(request.TeleSessionId, cancellationToken).ConfigureAwait(false);
        if (session is null || session.PatientId != patientId)
            throw new NotFoundException(nameof(TeleSession), request.TeleSessionId);

        var patient = await _patients.GetByIdAsync(patientId, cancellationToken).ConfigureAwait(false);
        if (patient is null || string.IsNullOrWhiteSpace(patient.PhoneNumber))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(Patient.PhoneNumber), "Add a phone number on your profile to receive SMS.")
            });
        }

        var channel = string.IsNullOrWhiteSpace(session.SessionExternalId)
            ? $"raph-tele-{session.Id:N}"
            : session.SessionExternalId.Trim();

        var message =
            $"RaphCare: telehealth session scheduled {session.ScheduledStart:u}. Open the app → Care & telehealth to join. Channel: {channel}";

        await _smsService.SendSmsAsync(patient.PhoneNumber.Trim(), message, cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
