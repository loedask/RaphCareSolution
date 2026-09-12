using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Appointments.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Appointments.Commands.AgreePatientAppointmentConsent;

public sealed class AgreePatientAppointmentConsentHandler(
    ICurrentUserService currentUser,
    IDateTimeProvider clock,
    IRepository<Appointment> appointmentRepository,
    IRepository<AppointmentConsent> consentRepository,
    IRepository<ClinicConsentTemplate> templateRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AgreePatientAppointmentConsentCommand, PatientAppointmentConsentDto>
{
    public async Task<PatientAppointmentConsentDto> Handle(
        AgreePatientAppointmentConsentCommand request,
        CancellationToken cancellationToken)
    {
        var patientId = currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required to agree to consent.");
        var userId = currentUser.CurrentUserId
            ?? throw new ForbiddenAccessException("Sign in is required to agree to consent.");

        var appointment = await appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken)
            .ConfigureAwait(false);
        if (appointment is null || appointment.PatientId != patientId || appointment.IsCancelled)
            throw new NotFoundException(nameof(Appointment), request.AppointmentId);

        var existingPage = await consentRepository.SearchAsync(
            q => q.Where(c => c.AppointmentId == appointment.Id),
            1,
            1,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        if (existingPage.Items.Count > 0)
        {
            var signed = existingPage.Items[0];
            return new PatientAppointmentConsentDto
            {
                NeedsConsent = false,
                Title = signed.TitleSnapshot,
                Body = signed.BodySnapshot,
                AlreadySigned = true,
                SignedAt = signed.SignedAt
            };
        }

        var templatePage = await templateRepository.SearchAsync(
            q => q.Where(t => t.ClinicId == appointment.ClinicId && t.IsActive),
            1,
            1,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        if (templatePage.Items.Count == 0)
            throw new BusinessRuleException("This practice has no active consent form to agree to.");

        var template = templatePage.Items[0];
        var consent = new AppointmentConsent
        {
            AppointmentId = appointment.Id,
            PatientId = patientId,
            ClinicId = appointment.ClinicId,
            TemplateId = template.Id,
            TitleSnapshot = template.Title,
            BodySnapshot = template.Body,
            SignedAt = clock.UtcNow,
            SignedByApplicationUserId = userId
        };

        await consentRepository.AddAsync(consent, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new PatientAppointmentConsentDto
        {
            NeedsConsent = false,
            Title = consent.TitleSnapshot,
            Body = consent.BodySnapshot,
            AlreadySigned = true,
            SignedAt = consent.SignedAt
        };
    }
}
