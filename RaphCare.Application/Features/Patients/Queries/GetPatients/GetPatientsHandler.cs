using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Patients.DTOs;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Patients.Queries.GetPatients;

/// <summary>
/// Handles paging over patients and mapping them to DTOs.
/// </summary>
public class GetPatientsHandler : IRequestHandler<GetPatientsQuery, PagedResult<PatientDto>>
{
    private readonly IRepository<Patient> _repository;

    /// <summary>
    /// Creates a handler instance.
    /// </summary>
    /// <param name="repository">Repository used to load patient entities.</param>
    public GetPatientsHandler(IRepository<Patient> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Retrieves a page of patients and returns the results plus paging metadata.
    /// </summary>
    /// <param name="request">Paging request (page number and size).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A paged result of <see cref="PatientDto"/>.</returns>
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

