using MediatR;
using RaphCare.Application.Features.Appointments.DTOs;

namespace RaphCare.Application.Features.Appointments.Commands.AgreePatientAppointmentConsent;

public sealed class AgreePatientAppointmentConsentCommand : IRequest<PatientAppointmentConsentDto>
{
    public Guid AppointmentId { get; init; }
}
