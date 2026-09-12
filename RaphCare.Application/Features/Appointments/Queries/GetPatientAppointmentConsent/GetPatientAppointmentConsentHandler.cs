using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Appointments.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Appointments.Queries.GetPatientAppointmentConsent;

public sealed class GetPatientAppointmentConsentHandler(
    ICurrentUserService currentUser,
    IRepository<Appointment> appointmentRepository,
    IRepository<AppointmentConsent> consentRepository,
    IRepository<ClinicConsentTemplate> templateRepository)
    : IRequestHandler<GetPatientAppointmentConsentQuery, PatientAppointmentConsentDto>
{
    public async Task<PatientAppointmentConsentDto> Handle(
        GetPatientAppointmentConsentQuery request,
        CancellationToken cancellationToken)
    {
        var patientId = currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required to view consent.");

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
        {
            return new PatientAppointmentConsentDto
            {
                NeedsConsent = false,
                AlreadySigned = false
            };
        }

        var template = templatePage.Items[0];
        return new PatientAppointmentConsentDto
        {
            NeedsConsent = true,
            Title = template.Title,
            Body = template.Body,
            AlreadySigned = false,
            SignedAt = null
        };
    }
}
