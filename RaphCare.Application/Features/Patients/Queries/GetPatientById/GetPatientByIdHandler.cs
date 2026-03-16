using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Patients.DTOs;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Patients.Queries.GetPatientById;

public class GetPatientByIdHandler(IRepository<Patient> repository) : IRequestHandler<GetPatientByIdQuery, PatientDto>
{
    private readonly IRepository<Patient> _repository = repository;

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

