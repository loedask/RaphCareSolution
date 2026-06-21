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
        var status = request.Status;
        if (!string.IsNullOrWhiteSpace(status))
            status = status.Trim();
        else
            status = null;

        var pagedAppointments = await _repository.SearchAsync(
            queryShaper: q =>
            {
                if (request.PatientId is Guid patientId)
                    q = q.Where(a => a.PatientId == patientId);

                if (status is not null)
                    q = q.Where(a => a.Status != null && string.Equals(a.Status, status, StringComparison.OrdinalIgnoreCase));

                return q;
            },
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        var entries = new List<FhirBundleEntryDto>();
        foreach (var appointment in pagedAppointments.Items)
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
            Total = pagedAppointments.TotalCount,
            Entry = entries
        };
    }
}

