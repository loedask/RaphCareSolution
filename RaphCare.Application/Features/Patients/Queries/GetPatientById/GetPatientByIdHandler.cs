using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Patients.DTOs;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Patients.Queries.GetPatientById;

/// <summary>
/// Retrieves a single patient by id and maps it to a data transfer object.
/// </summary>
public class GetPatientByIdHandler(IRepository<Patient> repository) : IRequestHandler<GetPatientByIdQuery, PatientDto>
{
    private readonly IRepository<Patient> _repository = repository;

    /// <summary>
    /// Handles the get-patient-by-id query.
    /// </summary>
    /// <param name="request">The query containing the patient id.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PatientDto"/> representing the patient.</returns>
    /// <exception cref="NotFoundException">Thrown when the patient does not exist.</exception>
    public async Task<PatientDto> Handle(GetPatientByIdQuery request, CancellationToken cancellationToken)
    {
        var patient = await _repository.GetByIdAsync(request.Id, cancellationToken);
        
        return patient is null
            ? throw new NotFoundException(nameof(Patient), request.Id)
            : new PatientDto
        {
            Id = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            DateOfBirth = patient.DateOfBirth
        };
    }
}

