using MediatR;
using RaphCare.Application.Features.PatientMentalHealth.DTOs;

namespace RaphCare.Application.Features.PatientMentalHealth.Queries.GetMyMoodCheckIns;

public sealed class GetMyMoodCheckInsQuery : IRequest<IReadOnlyList<PatientMoodCheckInDto>>
{
    public int PageSize { get; set; } = 14;
}
