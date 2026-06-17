using MediatR;
using RaphCare.Application.Features.Interoperability.DTOs;

namespace RaphCare.Application.Features.Interoperability.Queries.GetFhirAppointments;

/// <summary>
/// Phase 1: minimal searchset export for Appointment.
/// </summary>
public class GetFhirAppointmentsQuery : IRequest<FhirBundleDto>
{
    public Guid? PatientId { get; set; }
    public string? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

