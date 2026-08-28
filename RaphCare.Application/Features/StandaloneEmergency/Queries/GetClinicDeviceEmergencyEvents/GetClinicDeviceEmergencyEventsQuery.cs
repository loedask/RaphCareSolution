using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.StandaloneEmergency.DTOs;

namespace RaphCare.Application.Features.StandaloneEmergency.Queries.GetClinicDeviceEmergencyEvents;

public sealed class GetClinicDeviceEmergencyEventsQuery : IRequest<PagedResult<ClinicDeviceEmergencyEventListItemDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public DateTime? OccurredFromUtc { get; set; }
    public DateTime? OccurredToUtc { get; set; }
}
