using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.StandaloneEmergency.DTOs;

namespace RaphCare.Application.Features.StandaloneEmergency.Queries.GetPatientDeviceEmergencyEvents;

public sealed class GetPatientDeviceEmergencyEventsQuery : IRequest<PagedResult<PatientDeviceEmergencyEventListItemDto>>
{
    public Guid PatientId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public DateTime? OccurredFromUtc { get; set; }
    public DateTime? OccurredToUtc { get; set; }
}
