using MediatR;
using RaphCare.Application.Features.Appointments.DTOs;

namespace RaphCare.Application.Features.Appointments.Queries.GetPatientAppointmentConsent;

public sealed class GetPatientAppointmentConsentQuery : IRequest<PatientAppointmentConsentDto>
{
    public Guid AppointmentId { get; init; }
}
