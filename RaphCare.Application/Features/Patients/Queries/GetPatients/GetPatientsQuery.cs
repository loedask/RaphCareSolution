using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.Patients.DTOs;

namespace RaphCare.Application.Features.Patients.Queries.GetPatients;

public class GetPatientsQuery : IRequest<PagedResult<PatientDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

