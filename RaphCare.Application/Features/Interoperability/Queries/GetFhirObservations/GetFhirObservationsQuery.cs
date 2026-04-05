using MediatR;
using RaphCare.Application.Features.Interoperability.DTOs;

namespace RaphCare.Application.Features.Interoperability.Queries.GetFhirObservations;

/// <summary>Search device vitals as FHIR Observations (provider + current clinic).</summary>
public sealed class GetFhirObservationsQuery : IRequest<FhirBundleDto>
{
    public Guid PatientId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? ReadingType { get; set; }
    public DateTime? RecordedFromUtc { get; set; }
    public DateTime? RecordedToUtc { get; set; }
}
