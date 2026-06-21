using MediatR;
using Microsoft.Extensions.Options;
using RaphCare.Application.Common.Configuration;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.PatientSupport.Commands.SubmitPatientSupportMessage;

public sealed class SubmitPatientSupportMessageHandler(
    IRepository<Patient> patients,
    IRepository<PatientSupportMessage> messages,
    IEmailService emailService,
    IOptions<PatientSupportOptions> supportOptions,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<SubmitPatientSupportMessageCommand, Guid>
{
    private readonly IRepository<Patient> _patients = patients;
    private readonly IRepository<PatientSupportMessage> _messages = messages;
    private readonly IEmailService _emailService = emailService;
    private readonly PatientSupportOptions _supportOptions = supportOptions.Value;
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Guid> Handle(SubmitPatientSupportMessageCommand request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var patient = await _patients.GetByIdAsync(patientId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException(nameof(Patient), patientId);

        var subject = request.Subject.Trim();
        var message = request.Message.Trim();
        var entity = new PatientSupportMessage
        {
            PatientId = patientId,
            Subject = subject,
            Message = message,
            PatientEmail = string.IsNullOrWhiteSpace(patient.Email) ? null : patient.Email.Trim()
        };
        await _messages.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var supportEmail = _supportOptions.SupportEmail?.Trim();
        if (!string.IsNullOrWhiteSpace(supportEmail))
        {
            var emailSubject = $"{_supportOptions.InboundEmailSubjectPrefix} {subject}";
            var body =
                $"Patient: {patient.FirstName} {patient.LastName} ({patientId:D})\n" +
                $"Email: {entity.PatientEmail ?? "(none)"}\n" +
                $"Ticket: {entity.Id:D}\n\n" +
                message;
            await _emailService.SendEmailAsync(supportEmail, emailSubject, body, cancellationToken).ConfigureAwait(false);
        }

        return entity.Id;
    }
}
