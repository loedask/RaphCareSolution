using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.MentalHealth.DTOs;

namespace RaphCare.Application.Features.MentalHealth.Queries.GetTherapySessions;

public sealed class GetTherapySessionsQuery : IRequest<PagedResult<TherapySessionDto>>
{
    public Guid ClinicId { get; set; }
    public Guid? PatientId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
