using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Patients.DTOs;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Patients.Queries.GetPatients;

public class GetPatientsHandler : IRequestHandler<GetPatientsQuery, PagedResult<PatientDto>>
{
    private readonly IRepository<Patient> _repository;

    public GetPatientsHandler(IRepository<Patient> repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<PatientDto>> Handle(GetPatientsQuery request, CancellationToken cancellationToken)
    {
        var patients = await _repository.ListAsync(cancellationToken);

        var totalCount = patients.Count;

        var items = patients
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PatientDto
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                DateOfBirth = p.DateOfBirth
            })
            .ToList();

        return new PagedResult<PatientDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

