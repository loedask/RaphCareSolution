using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.Clinical.DTOs;

namespace RaphCare.Application.Features.Clinical.Queries.GetPatientDeviceReadings;

/// <summary>Provider-scoped list of device vitals for a patient (current clinic from <see cref="Common.Interfaces.IClinicContext"/>).</summary>
public sealed class GetPatientDeviceReadingsQuery : IRequest<PagedResult<PatientDeviceReadingListItemDto>>
{
    public Guid PatientId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    /// <summary>Optional filter on stored reading type (e.g. HeartRate, SpO2).</summary>
    public string? ReadingType { get; set; }
    public DateTime? RecordedFromUtc { get; set; }
    public DateTime? RecordedToUtc { get; set; }
}
