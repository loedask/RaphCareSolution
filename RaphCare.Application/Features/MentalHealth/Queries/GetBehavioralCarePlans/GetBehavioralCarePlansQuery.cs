using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.MentalHealth.DTOs;

namespace RaphCare.Application.Features.MentalHealth.Queries.GetBehavioralCarePlans;

public sealed class GetBehavioralCarePlansQuery : IRequest<PagedResult<BehavioralCarePlanDto>>
{
    public Guid ClinicId { get; set; }
    public Guid? PatientId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
