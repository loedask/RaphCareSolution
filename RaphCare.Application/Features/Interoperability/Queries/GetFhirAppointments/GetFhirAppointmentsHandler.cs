using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Interoperability.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Interoperability.Queries.GetFhirAppointments;

/// <summary>
/// Phase 1: minimal searchset export for Appointment.
/// </summary>
public class GetFhirAppointmentsHandler(
    IRepository<Appointment> repository,
    IAppointmentFhirMapper mapper) : IRequestHandler<GetFhirAppointmentsQuery, FhirBundleDto>
{
    private readonly IRepository<Appointment> _repository = repository;
    private readonly IAppointmentFhirMapper _mapper = mapper;

    public async Task<FhirBundleDto> Handle(GetFhirAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var appointments = await _repository.ListAsync(cancellationToken).ConfigureAwait(false);

        var filtered = appointments.AsEnumerable();

        if (request.PatientId is Guid patientId)
            filtered = filtered.Where(a => a.PatientId == patientId);

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = request.Status.Trim();
            filtered = filtered.Where(a => string.Equals(a.Status, status, StringComparison.OrdinalIgnoreCase));
        }

        var entries = new List<FhirBundleEntryDto>();
        foreach (var appointment in filtered)
        {
            var dto = await _mapper.MapToDtoAsync(appointment, cancellationToken).ConfigureAwait(false);
            entries.Add(new FhirBundleEntryDto
            {
                FullUrl = $"urn:raphcare:fhir:Appointment/{appointment.Id}",
                Resource = dto
            });
        }

        return new FhirBundleDto
        {
            Total = entries.Count,
            Entry = entries
        };
    }
}

